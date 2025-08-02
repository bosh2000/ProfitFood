using AutoMapper;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Model.DBModel;
using ProfitFood.Model.Infrastructure;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using ProfitFood.UI.Views.BaseUnit;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.BaseUnitViewModels
{
    /// <summary>
    /// Вкладка Базовая единица измерения в Tab в основом экране
    /// </summary>
    public class BaseUnitTabViewModel : ViewModel
    {
        private readonly IProfitDbRepository _profitDbRepository;
        private readonly IMapper _mapper;

        public ObservableCollection<BaseUnitItemView> BaseUnits { get; } = new ObservableCollection<BaseUnitItemView>();
        private BaseUnitItemView _selectedBaseUnit;

        public BaseUnitItemView SelectedBaseUnit
        {
            get => _selectedBaseUnit;
            set
            {
                _selectedBaseUnit = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public string SearchBaseUnit { get; set; }

        public BaseUnitTabViewModel(IProfitDbRepository repository, IMapper mapper)
        {
            _profitDbRepository = repository;
            _mapper = mapper;
            AddProductCommand = new LambdaCommand(AddBaseUnitItem);
            DeleteProductCommand = new LambdaCommandAsync(DeleteBaseUnitItem, CanEditDelete);
            EditProductCommand = new LambdaCommand(EditBaseUnitItem, CanEditDelete);
            LoadBaseUnits();
        }

        private async void LoadBaseUnits()
        {
            BaseUnits.Clear();

            var units = await _profitDbRepository.BaseUnitRepository.ToListAsync();
            foreach (var unit in units)
                BaseUnits.Add(_mapper.Map<BaseUnitItemView>(unit));
        }

        private async void AddBaseUnitItem(object param)
        {
            var addBaseUnitWindows = new BaseUnitItemWindow();
            var viewModel = (BaseUnitItemWindowViewModel)addBaseUnitWindows.DataContext;
            viewModel.BaseUnitCreated += async baseUnit =>
            {
                var result = BaseUnit.Create(baseUnit.Name);
                if (!result.IsSuccess)
                {
                    ShowErrors(result.Errors);
                    return;
                }
                var baseUnitCreated = await _profitDbRepository.BaseUnitRepository.CreateASync(result.Value);
                var baseUnitView = _mapper.Map<BaseUnitItemView>(baseUnitCreated);
                BaseUnits.Add(baseUnitView);
                SelectedBaseUnit = baseUnitView;
                addBaseUnitWindows.Close();
            };
            addBaseUnitWindows.Owner = Application.Current.MainWindow;
            addBaseUnitWindows.ShowDialog();
        }

        private async Task DeleteBaseUnitItem(object param)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы уверенны что хотите удалить запись?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var baseUnit = await _profitDbRepository.BaseUnitRepository.FirstOfDefaultAsync(x => x.Id == SelectedBaseUnit.Id);
                if (baseUnit != null)
                {
                    await _profitDbRepository.BaseUnitRepository.DeleteAsync(baseUnit);
                    BaseUnits.Remove(SelectedBaseUnit);
                }
            }
        }

        private void EditBaseUnitItem(object param)
        {
        }

        // TODO Добавить вывод ошибок в StatusBar
        private void ShowErrors(IReadOnlyCollection<Error> errors)
        {
        }

        private bool CanEditDelete(object param)
        { return SelectedBaseUnit != null; }
    }
}