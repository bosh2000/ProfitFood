using ProfitFood.UI.ViewModels.Reference.ProductCategories;
using System.Windows;
using System.Windows.Controls;

namespace ProfitFood.UI.Views.References
{
    /// <summary>
    /// Логика взаимодействия для ProductCategoriesReferenceView.xaml
    /// </summary>
    public partial class ProductCategoriesView : UserControl
    {
        public ProductCategoriesView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(
                object sender,
                RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ProductCategoriesViewModel vm &&
                e.NewValue is ProductCategoryTreeItemViewModel item)
            {
                vm.SelectedCategory = item;
            }
        }
    }
}