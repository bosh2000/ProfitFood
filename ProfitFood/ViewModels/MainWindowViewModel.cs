using ProfitFood.UI.ViewModels.DailyMenuEditorViewModels;
using ProfitFood.UI.ViewModels.DashBoardViewModels;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        private string _statusText = "Готово";

        public MainWindowViewModel()
        {
            OpenDashboardCommand = new RelayCommand(_ => OpenDashboard());
            OpenDailyMenusCommand = new RelayCommand(_ => OpenDailyMenus());
            OpenDailyMenuEditorCommand = new RelayCommand(_ => OpenDailyMenuEditor());
            OpenRecipeCardsCommand = new RelayCommand(_ => OpenRecipeCards());
            OpenStockCommand = new RelayCommand(_ => OpenStock());
            OpenDocumentsCommand = new RelayCommand(_ => OpenDocuments());
            OpenReferencesCommand = new RelayCommand(_ => OpenReferences());

            OpenDashboard();
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ICommand OpenDashboardCommand { get; }
        public ICommand OpenDailyMenusCommand { get; }
        public ICommand OpenDailyMenuEditorCommand { get; }
        public ICommand OpenRecipeCardsCommand { get; }
        public ICommand OpenStockCommand { get; }
        public ICommand OpenDocumentsCommand { get; }
        public ICommand OpenReferencesCommand { get; }

        private void OpenDashboard()
        {
            CurrentViewModel = new DashboardViewModel();
            StatusText = "Открыта главная страница";
        }

        private void OpenDailyMenus()
        {
            CurrentViewModel = new DailyMenuListViewModel();
            StatusText = "Открыт список меню";
        }

        private void OpenDailyMenuEditor()
        {
            CurrentViewModel = new DailyMenuEditorViewModel();
            StatusText = "Открыт редактор меню";
        }

        private void OpenRecipeCards()
        {
            CurrentViewModel = new RecipeCardsViewModel();
            StatusText = "Открыт раздел техкарт";
        }

        private void OpenStock()
        {
            CurrentViewModel = new StockViewModel();
            StatusText = "Открыт склад";
        }

        private void OpenDocuments()
        {
            CurrentViewModel = new DocumentsViewModel();
            StatusText = "Открыт журнал документов";
        }

        private void OpenReferences()
        {
            CurrentViewModel = new ReferencesViewModel();
            StatusText = "Открыт раздел справочников";
        }
    }
}