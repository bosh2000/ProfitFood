using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.ViewModels.Base;
using ProfitFood.UI.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public ObservableCollection<TabItemViewModel> TabItems { get; set; } = new();

        private TabItemViewModel _selectedTab;

        public TabItemViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenTabCommand { get; }
        public ICommand CloseTabCommand { get; }

        public string TitleWindows
        { get; } = "Profit Питание";

        private readonly IProfitDbRepository _profitDbRepository;

        public MainWindowViewModel(IProfitDbRepository repository)
        {
            _profitDbRepository = repository;
            OpenTabCommand = new LambdaCommand(
                Execute: OpenNewTab,
                CanExecute: _ => true
                );
            CloseTabCommand = new LambdaCommand(
                Execute: CloseTab,
                CanExecute: _ => true);
        }

        public void OpenNewTab(object param)
        {
            if (param is not string tabType)
                return;
            var existingTab = TabItems.FirstOrDefault(x => x.Header == tabType);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
                return;
            }
            var content = GetContentForTab(tabType);
            var newTab = new TabItemViewModel(tabType, content, CloseTab);
            TabItems.Add(newTab);
            SelectedTab = newTab;
        }

        private void CloseTab(object param)
        {
            if (param is null)
            { return; }
            if (param is TabItemViewModel tabItemViewModel)
                if (tabItemViewModel != null)
                    TabItems.Remove(tabItemViewModel);
        }

        private object GetContentForTab(string tabName)
        {
            return tabName switch
            {
                "Products" => new ProductsView(),
                //  "ProductTypes" => new ProductTypesView(),
                //  "BaseUnits" => new BaseUnitsView(),
                _ => new TextBlock { Text = $"Контент для {tabName}" }
            };
        }
    }
}