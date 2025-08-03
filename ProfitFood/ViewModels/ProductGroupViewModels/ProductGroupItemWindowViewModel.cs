using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.ProductGroupViewModels
{
    public class ProductGroupItemWindowViewModel : ViewModel
    {
        private string _productGroupName;
        private string _productGroupDescription;

        public string ProductGroupName
        {
            get => _productGroupName;
            set
            {
                _productGroupName = value;
                OnPropertyChanged();
            }
        }

        public string ProductGroupDescription
        {
            get => _productGroupDescription;
            set
            {
                _productGroupDescription = value;
                OnPropertyChanged();
            }
        }

        public event Action<ProductGroupItemView> CreateProductGroup;

        public event Action<ProductGroupItemView> UpdateProductGroup;

        public bool _isEditMode;
        private ProductGroupItemView _editProductGroup;

        public void InitializeEdit(ProductGroupItemView editProductGroup)
        {
            _isEditMode = true;
            _editProductGroup = editProductGroup;
            ProductGroupName = editProductGroup.Name;
            ProductGroupDescription = editProductGroup.Description;
        }

        public ICommand SaveProductGroupCommand { get; }

        public ProductGroupItemWindowViewModel()
        {
            SaveProductGroupCommand = new LambdaCommand(SaveProductGroup, CanSaveProductGroupItem);
        }

        private void SaveProductGroup(object param)
        {
            var productGroup = new ProductGroupItemView
            {
                Description = ProductGroupDescription,
                Name = ProductGroupName,
                Id = _isEditMode ? _editProductGroup.Id : Guid.Empty,
            };
            if (_isEditMode)
                UpdateProductGroup?.Invoke(productGroup);
            else
                CreateProductGroup?.Invoke(productGroup);
        }

        private bool CanSaveProductGroupItem(object param)
        {
            return !string.IsNullOrWhiteSpace(ProductGroupName) && !string.IsNullOrWhiteSpace(ProductGroupDescription);
        }
    }
}