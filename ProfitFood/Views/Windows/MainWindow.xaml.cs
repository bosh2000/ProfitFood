using ProfitFood.UI.ViewModels;
using System;
using System.Windows;

namespace ProfitFood.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}