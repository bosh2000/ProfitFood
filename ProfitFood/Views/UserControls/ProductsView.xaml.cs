using ProfitFood.UI.ViewModels;
using System.Windows.Controls;

namespace ProfitFood.UI.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ProductsView.xaml
    /// </summary>
    public partial class ProductsView : UserControl
    {
        public ProductsView(ProductTabViewModel productTabViewModel)
        {
            DataContext = productTabViewModel;
            InitializeComponent();
        }
    }
}