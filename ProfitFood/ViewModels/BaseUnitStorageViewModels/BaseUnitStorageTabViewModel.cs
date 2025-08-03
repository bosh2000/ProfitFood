using AutoMapper;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Model.DBModel;
using ProfitFood.Model.Infrastructure;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using ProfitFood.UI.Views.BaseUnitStorage;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.BaseUnitStorageViewModels
{
    public class BaseUnitStorageTabViewModel : ViewModel
    {
        private readonly IProfitDbRepository _repository;
        private readonly IMapper _mapper;

        public ICommand AddBaseUnitStorageCommand { get; }
        public ICommand DeleteBaseUnitStorageCommand { get; }
        public ICommand EditBaseUnitStorageCommand { get; }

        public ObservableCollection<BaseUnitStorageItemView> BaseUnitsStorage { get; } = new ObservableCollection<BaseUnitStorageItemView>();

        private BaseUnitStorageItemView _selectedBaseUnitStorage;

        public string SearchBaseUnitStorage { get; set; }

        public BaseUnitStorageItemView SelectedBaseUnitStorage
        {
            get => _selectedBaseUnitStorage;
            set
            {
                _selectedBaseUnitStorage = value;
                OnPropertyChanged();
            }
        }

        public BaseUnitStorageTabViewModel(IProfitDbRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
            LoadBaseUnitStorage();
            AddBaseUnitStorageCommand = new LambdaCommand(AddBaseUnitStorage);
            DeleteBaseUnitStorageCommand = new LambdaCommand(DeleteBaseUnitStorage, CanEditDelete);
            EditBaseUnitStorageCommand = new LambdaCommand(EditBaseUnitStorage, CanEditDelete);
        }

        private async Task LoadBaseUnitStorage()
        {
            BaseUnitsStorage.Clear();
            var units = await _repository.BaseUnitStorageRepository.ToListAsync();
            foreach (var unit in units)
            {
                BaseUnitsStorage.Add(_mapper.Map<BaseUnitStorageItemView>(unit));
            }
        }

        private async void DeleteBaseUnitStorage(object param)
        {
            MessageBoxResult result = MessageBox.Show(
                    "Вы уверенны что хотите удалить запись?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var baseUnitStorage = await _repository.BaseUnitStorageRepository.FirstOfDefaultAsync(x => x.Id == SelectedBaseUnitStorage.Id);
                if (baseUnitStorage != null)
                {
                    await _repository.BaseUnitStorageRepository.DeleteAsync(baseUnitStorage);
                    BaseUnitsStorage.Remove(SelectedBaseUnitStorage);
                }
            }
        }

        private async void AddBaseUnitStorage(object param)
        {
            var addBaseUnitStorageWindows = new BaseUnitStorageItemWindow();
            var viewModel = (BaseUnitStorageItemWindowViewModel)addBaseUnitStorageWindows.DataContext;
            viewModel.BaseUnitStorageItemCreate += async baseUnitStorage =>
            {
                var result = BaseUnitStorage.Create(baseUnitStorage.Name, baseUnitStorage.Description);
                if (!result.IsSuccess)
                {
                    ShowErrors(result.Errors);
                    return;
                }
                var baseUnitStorageCreated = await _repository.BaseUnitStorageRepository.CreateASync(result.Value);
                var baseUnitStrageView = _mapper.Map<BaseUnitStorageItemView>(baseUnitStorageCreated);
                BaseUnitsStorage.Add(baseUnitStrageView);
                SelectedBaseUnitStorage = baseUnitStorage;
                addBaseUnitStorageWindows.Close();
            };
            addBaseUnitStorageWindows.Owner = Application.Current.MainWindow;
            addBaseUnitStorageWindows.ShowDialog();
        }

        private void EditBaseUnitStorage(object param)
        {
            if (SelectedBaseUnitStorage == null) return;

            var baseUnitStorageWindow = new BaseUnitStorageItemWindow();
            var viewModel = (BaseUnitStorageItemWindowViewModel)baseUnitStorageWindow.DataContext;
            viewModel.InitializeEdit(SelectedBaseUnitStorage);
            viewModel.BaseUnitStorageItemUpdate += async baseUnitStorage =>
            {
                var item = await _repository.BaseUnitStorageRepository.FirstOfDefaultAsync(x => x.Id == baseUnitStorage.Id);
                if (item != null)
                {
                    var result = item.SetName(baseUnitStorage.Name);
                    if (!result.IsSuccess)
                    {
                        ShowErrors(result.Errors);
                        return;
                    }
                    result = item.SetDescription(baseUnitStorage.Description);
                    if (!result.IsSuccess)
                    {
                        ShowErrors(result.Errors);
                        return;
                    }
                    await _repository.BaseUnitStorageRepository.UpdateAsync(item);
                    var index = BaseUnitsStorage.IndexOf(SelectedBaseUnitStorage);
                    BaseUnitsStorage[index] = _mapper.Map<BaseUnitStorageItemView>(item);
                }
                baseUnitStorageWindow.Close();
            };
            baseUnitStorageWindow.Owner = Application.Current.MainWindow;
            baseUnitStorageWindow.ShowDialog();
        }

        private void ShowErrors(IReadOnlyCollection<Error> errors)
        {
        }

        private bool CanEditDelete(object param)
        {
            return SelectedBaseUnitStorage != null;
        }
    }
}