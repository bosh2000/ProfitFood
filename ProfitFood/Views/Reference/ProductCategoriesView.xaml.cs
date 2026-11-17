using ProfitFood.UI.ViewModels.Reference.ProductCategories;
using System.Windows;
using System.Windows.Controls;

namespace ProfitFood.UI.Views.Reference
{
    /// <summary>
    /// Логика взаимодействия для ProductCategoriesView.xaml
    /// </summary>
    public partial class ProductCategoriesView : UserControl
    {
        /// <summary>
        /// Инициализирует представление справочника категорий продуктов.
        /// </summary>
        public ProductCategoriesView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Передаёт выбранный узел дерева в ViewModel: TreeView не умеет привязывать SelectedItem напрямую.
        /// </summary>
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
