using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Вкладка Базовая единица измерения в Tab в основом экране
    /// </summary>
    public class BaseUnitTabViewModel : ViewModel
    {
        private readonly IProfitDbRepository _profitDbRepository;

        public ObservableCollection<BaseUnitView> BaseUnits { get; } = new ObservableCollection<BaseUnitView>();
        private BaseUnitView _selectedBaseUnit;

        public BaseUnitView SelectedBaseUnit
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
            this._profitDbRepository = repository;
            AddProductCommand = new LambdaCommand(AddProduct);
            DeleteProductCommand = new LambdaCommand(DeleteProduct, CanEditDelete);
            EditProductCommand = new LambdaCommand(EditProduct, CanEditDelete);
            LoadBaseUnits();
        }

        private void LoadBaseUnits()
        {
            BaseUnits.Clear();
            BaseUnits.Add(new BaseUnitView
            {
                Id = Guid.NewGuid(),
                Name = "BaseUnit1",
            });
            BaseUnits.Add(new BaseUnitView
            {
                Id = Guid.NewGuid(),
                Name = "BaseUnit2",
            });
            BaseUnits.Add(new BaseUnitView
            {
                Id = Guid.NewGuid(),
                Name = "BaseUnit3",
            });
        }

        private void AddProduct(object param)
        {
        }

        private void DeleteProduct(object param)
        {
        }

        private void EditProduct(object param)
        {
        }

        private bool CanEditDelete(object param)
        { return SelectedBaseUnit != null; }
    }
}