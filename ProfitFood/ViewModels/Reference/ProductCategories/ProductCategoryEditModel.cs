using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.ViewModels.Reference.ProductCategories
{
    public sealed class ProductCategoryEditModel : ViewModelBase
    {
        private Guid _id;
        private string _name = string.Empty;
        private Guid? _parentCategoryId;
        private int _sortOrder;

        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Guid? ParentCategoryId
        {
            get => _parentCategoryId;
            set => SetProperty(ref _parentCategoryId, value);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetProperty(ref _sortOrder, value);
        }

        public bool IsNew => Id == Guid.Empty;
    }
}