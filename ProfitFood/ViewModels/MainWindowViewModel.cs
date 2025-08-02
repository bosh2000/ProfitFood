using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.ViewModels.Base;
using ProfitFood.UI.ViewModels.BaseUnitViewModels;
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
        private readonly IProfitDbRepository _profitDbRepository;
        private readonly IServiceProvider _serviceProvider;
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

        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _profitDbRepository = _serviceProvider.GetRequiredService<IProfitDbRepository>();// repository;
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
            var existingTab = TabItems.FirstOrDefault(x => x.Id == tabType);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
                return;
            }
            var content = GetContentForTab(tabType);
            var tabHeader = GetTabHeaderByType(tabType);
            var newTab = new TabItemViewModel(tabType, tabHeader, content, CloseTab);
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

        private string GetTabHeaderByType(string tabType)
        {
            return tabType switch
            {
                "Products" => "Продукты",
                "TypeProduct" => "Тип продуктов",
                "BaseUnit" => "Базовая единица измерения",
                "WarehouseUnit" => "Складская единица измерения",
                "AccountingUnits" => "Учетная единица измерения",
                _ => "Unknow tabtype"
            };
        }

        private object GetContentForTab(string tabName)
        {
            switch (tabName)
            {
                case "Products":
                    var productVm = new ProductTabViewModel(_profitDbRepository);
                    return new ProductsView(productVm);

                case "BaseUnit":
                    var baseUnitVm = new BaseUnitTabViewModel(_profitDbRepository
                        , _serviceProvider.GetRequiredService<IMapper>());
                    return new BaseUnitsView(baseUnitVm);

                default:
                    return new TextBlock { Text = $"Контент для {tabName}" };
            }
        }
    }
}