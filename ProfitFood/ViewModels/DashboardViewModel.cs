using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Главная панель системы.
    /// Показывает состояние питания на сегодня и быстрые действия.
    /// </summary>
    public sealed class DashboardViewModel : ViewModelBase
    {
        private DateTime _todayDate;
        private int _totalChildren;
        private int _dishCount;
        private int _productCount;
        private string _menuStatus = string.Empty;
        private string _statusMessage = "Готово";

        public DashboardViewModel()
        {
            TodayMenuItems = new ObservableCollection<DashboardMenuItemViewModel>();
            CriticalStockProducts = new ObservableCollection<CriticalStockRowViewModel>();
            LastDocuments = new ObservableCollection<LastDocumentRowViewModel>();
            Warnings = new ObservableCollection<string>();

            CreateMenuCommand = new RelayCommand(_ => StatusMessage = "Переход к созданию меню на сегодня");
            OpenTodayMenuCommand = new RelayCommand(_ => StatusMessage = "Открыта карточка меню на сегодня");
            CalculateProductsCommand = new RelayCommand(_ => StatusMessage = "Запущен расчет продуктов");
            CreateRequirementCommand = new RelayCommand(_ => StatusMessage = "Создано меню-требование");
            PrintRequirementCommand = new RelayCommand(_ => StatusMessage = "Открыт предпросмотр формы 299");

            LoadDesignData();
        }

        public static DashboardViewModel DesignInstance => new DashboardViewModel();

        public RelayCommand CreateMenuCommand { get; }
        public RelayCommand OpenTodayMenuCommand { get; }
        public RelayCommand CalculateProductsCommand { get; }
        public RelayCommand CreateRequirementCommand { get; }
        public RelayCommand PrintRequirementCommand { get; }

        public ObservableCollection<DashboardMenuItemViewModel> TodayMenuItems { get; }
        public ObservableCollection<CriticalStockRowViewModel> CriticalStockProducts { get; }
        public ObservableCollection<LastDocumentRowViewModel> LastDocuments { get; }
        public ObservableCollection<string> Warnings { get; }

        public DateTime TodayDate
        {
            get => _todayDate;
            set
            {
                if (SetProperty(ref _todayDate, value))
                {
                    OnPropertyChanged(nameof(TodayDateText));
                }
            }
        }

        public string TodayDateText => TodayDate.ToString("dd.MM.yyyy");

        public int TotalChildren
        {
            get => _totalChildren;
            set => SetProperty(ref _totalChildren, value);
        }

        public int DishCount
        {
            get => _dishCount;
            set => SetProperty(ref _dishCount, value);
        }

        public int ProductCount
        {
            get => _productCount;
            set => SetProperty(ref _productCount, value);
        }

        public string MenuStatus
        {
            get => _menuStatus;
            set
            {
                if (SetProperty(ref _menuStatus, value))
                {
                    OnPropertyChanged(nameof(MenuStatusBrush));
                }
            }
        }

        public Brush MenuStatusBrush => MenuStatus switch
        {
            "Рассчитано" => Brushes.ForestGreen,
            "Черновик" => Brushes.DarkGoldenrod,
            "Проблема" => Brushes.IndianRed,
            _ => Brushes.SteelBlue
        };

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private void LoadDesignData()
        {
            TodayDate = new DateTime(2026, 3, 12);
            TotalChildren = 126;
            MenuStatus = "Рассчитано";

            TodayMenuItems.Clear();
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Завтрак", "Каша молочная", "ТК-054", "200 г"));
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Завтрак", "Чай с сахаром", "ТК-012", "200 мл"));
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Обед", "Суп овощной", "ТК-083", "250 г"));
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Обед", "Котлета мясная", "ТК-092", "90 г"));
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Обед", "Компот", "ТК-034", "200 мл"));
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Полдник", "Булочка", "ТК-121", "60 г"));
            TodayMenuItems.Add(new DashboardMenuItemViewModel("Полдник", "Кефир", "ТК-140", "180 мл"));

            DishCount = TodayMenuItems.Count;
            ProductCount = 38;

            CriticalStockProducts.Clear();
            CriticalStockProducts.Add(new CriticalStockRowViewModel("Молоко", "2 л", "5 л"));
            CriticalStockProducts.Add(new CriticalStockRowViewModel("Яйцо", "15 шт", "30 шт"));
            CriticalStockProducts.Add(new CriticalStockRowViewModel("Масло сливочное", "0.8 кг", "1.5 кг"));

            LastDocuments.Clear();
            LastDocuments.Add(new LastDocumentRowViewModel("Меню-требование №12", "12.03.2026", "Создано"));
            LastDocuments.Add(new LastDocumentRowViewModel("Выдача со склада №8", "12.03.2026", "Проведен"));
            LastDocuments.Add(new LastDocumentRowViewModel("Меню-требование №11", "11.03.2026", "Закрыто"));

            Warnings.Clear();
            Warnings.Add("Не хватает молока для полного выполнения меню.");
            Warnings.Add("Есть критически низкие остатки по 3 продуктам.");
            Warnings.Add("Документ выдачи со склада за сегодня еще не проведен.");

            StatusMessage = "Загружены тестовые данные Dashboard";
        }
    }

    /// <summary>
    /// Строка меню на сегодня.
    /// </summary>
    public sealed class DashboardMenuItemViewModel
    {
        public DashboardMenuItemViewModel(string mealTypeName, string dishName, string recipeCardNumber, string outputText)
        {
            MealTypeName = mealTypeName;
            DishName = dishName;
            RecipeCardNumber = recipeCardNumber;
            OutputText = outputText;
        }

        public string MealTypeName { get; }
        public string DishName { get; }
        public string RecipeCardNumber { get; }
        public string OutputText { get; }
    }

    /// <summary>
    /// Строка критического остатка.
    /// </summary>
    public sealed class CriticalStockRowViewModel
    {
        public CriticalStockRowViewModel(string productName, string stockText, string minStockText)
        {
            ProductName = productName;
            StockText = stockText;
            MinStockText = minStockText;
        }

        public string ProductName { get; }
        public string StockText { get; }
        public string MinStockText { get; }
    }

    /// <summary>
    /// Строка последнего документа.
    /// </summary>
    public sealed class LastDocumentRowViewModel
    {
        public LastDocumentRowViewModel(string documentName, string dateText, string statusName)
        {
            DocumentName = documentName;
            DateText = dateText;
            StatusName = statusName;
        }

        public string DocumentName { get; }
        public string DateText { get; }
        public string StatusName { get; }
    }
}