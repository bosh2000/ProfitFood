using ProfitFood.UI.ViewModels;
using System;
using System.Windows;

namespace ProfitFood.UI
{
    /// <summary>
    /// =ЛОгика работы MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}