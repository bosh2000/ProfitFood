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

        public ObservableCollection<BaseUnitItemView> BaseUnits { get; } = new ObservableCollection<BaseUnitItemView>();
        private BaseUnitItemView _selectedBaseUnit;

        public BaseUnitItemView SelectedBaseUnit
        {
            get { return _selectedBaseUnit; }
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

        public BaseUnitTabViewModel(IProfitDbRepository repository)
        {
            _profitDbRepository = repository;
            AddProductCommand = new LambdaCommand(AddProduct);
            DeleteProductCommand = new LambdaCommand(DeleteProduct, CanEditDelete);
            EditProductCommand = new LambdaCommand(EditProduct, CanEditDelete);
            LoadBaseUnits();
        }

        private async void LoadBaseUnits()
        {
            BaseUnits.Clear();

            var units = await _profitDbRepository.BaseUnitRepository.ToListAsync();
            foreach (var unit in units)
                BaseUnits.Add(new BaseUnitItemView { Id = unit.Id, Name = unit.Name });
        }

        private async void AddProduct(object param)
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
                var baseUnitView =
                                    new BaseUnitItemView
                                    {
                                        Id = baseUnitCreated.Id,
                                        Name = baseUnitCreated.Name
                                    };
                BaseUnits.Add(baseUnitView);
                SelectedBaseUnit = baseUnitView;
                addBaseUnitWindows.Close();
            };
            addBaseUnitWindows.Owner = Application.Current.MainWindow;
            addBaseUnitWindows.ShowDialog();
        }

        private void DeleteProduct(object param)
        {
        }

        private void EditProduct(object param)
        {
        }

        private void ShowErrors(IReadOnlyCollection<Error> errors)
        {
        }

        private bool CanEditDelete(object param)
        { return SelectedBaseUnit != null; }
    }
}