using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.ViewModels.Reference.ProductCategories
{
    public sealed class ProductCategoryTreeItemViewModel : ViewModelBase
    {
        private bool _isExpanded;
        private bool _isSelected;

        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid? ParentCategoryId { get; set; }

        public int SortOrder { get; set; }

        public ObservableCollection<ProductCategoryTreeItemViewModel> Children { get; } = new();

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public override string ToString() => Name;
    }
}