using ProfitFood.UI.ViewModels.ProductGroupViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProfitFood.UI.Views.ProductGroup
{
    /// <summary>
    /// Логика взаимодействия для ProductGroupsView.xaml
    /// </summary>
    public partial class ProductGroupsView : UserControl
    {
        public ProductGroupsView(ProductGroupTabViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}