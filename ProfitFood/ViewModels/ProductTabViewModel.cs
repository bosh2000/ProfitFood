using Microsoft.EntityFrameworkCore.Metadata;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Вкладка Продукты в Tab в основном экране
    /// </summary>
    public class ProductTabViewModel : ViewModel
    {
        public ObservableCollection<ProductView> Products { get; } = new ObservableCollection<ProductView>();
        private ProductView _selectedProduct;
        private readonly IProfitDbRepository _profitDbRepository;

        public ProductView SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public string SearchProduct { get; set; }

        public ProductTabViewModel(IProfitDbRepository profitDbRepository)
        {
            AddProductCommand = new LambdaCommand(AddProduct);
            DeleteProductCommand = new LambdaCommand(DeleteProduct, CanEditDelete);
            EditProductCommand = new LambdaCommand(EditProduct, CanEditDelete);
            _profitDbRepository = profitDbRepository;

            LoadProducts();
        }

        private void LoadProducts()
        {
            Products.Clear();
            Products.Add(new ProductView
            {
                Id = Guid.NewGuid(),
                Name = "Product1",
                BaseUnit = "Шт",
                BaseUnitStorage = "Упаковка",
                Description = "Новый продукт",
                FullName = "FullNamePRoduct",
                Group = "ГруппаПродукта"
            });
            Products.Add(new ProductView
            {
                Id = Guid.NewGuid(),
                Name = "Product2",
                BaseUnit = "Шт",
                BaseUnitStorage = "Упаковка",
                Description = "Новый продукт",
                FullName = "FullNamePRoduct2",
                Group = "ГруппаПродукта"
            });
            Products.Add(new ProductView
            {
                Id = Guid.NewGuid(),
                Name = "Product3",
                BaseUnit = "Шт",
                BaseUnitStorage = "Упаковка",
                Description = "Новый продукт",
                FullName = "FullNamePRoduct3",
                Group = "ГруппаПродукта"
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
        { return SelectedProduct != null; }
    }
}