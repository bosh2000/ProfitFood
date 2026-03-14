using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitFood.UI.ViewModels.DailyMenuEditorViewModels
{
    /// <summary>
    /// ViewModel редактора ежедневного меню.
    /// Используется экраном DailyMenuEditorView.

    /// </summary>
    public sealed class DailyMenuEditorViewModel : ViewModelBase
    {
        private DateTime? _menuDate;
        private SeasonItemViewModel? _selectedSeason;
        private CycleMenuItemViewModel? _selectedCycleMenu;
        private string? _selectedStatus;
        private string _notes = string.Empty;
        private string _sourceDescription = string.Empty;
        private string _createdAtText = string.Empty;
        private string _updatedAtText = string.Empty;
        private int _totalChildrenCount;
        private int _dishCount;
        private bool _hasDeficit;
        private DishRowViewModel? _selectedDishItem;
        private CalculatedProductRowViewModel? _selectedCalculatedProduct;
        private RelatedDocumentRowViewModel? _selectedRelatedDocument;
        private string _statusMessage = "Готово";

        public DailyMenuEditorViewModel()
        {
            Seasons = new ObservableCollection<SeasonItemViewModel>();
            CycleMenus = new ObservableCollection<CycleMenuItemViewModel>();
            AvailableStatuses = new ObservableCollection<string>();
            Attendances = new ObservableCollection<AttendanceRowViewModel>();
            Dishes = new ObservableCollection<DishRowViewModel>();
            CalculatedProducts = new ObservableCollection<CalculatedProductRowViewModel>();
            RelatedDocuments = new ObservableCollection<RelatedDocumentRowViewModel>();

            BackCommand = new RelayCommand(_ => StatusMessage = "Возврат к списку меню");
            SaveCommand = new RelayCommand(_ => StatusMessage = "Изменения сохранены");
            CalculateProductsCommand = new RelayCommand(_ => RecalculateProducts());
            CreateRequirementCommand = new RelayCommand(_ => StatusMessage = "Создано меню-требование");
            PrintCommand = new RelayCommand(_ => StatusMessage = "Открыт предпросмотр печати");
            FillFromCycleMenuCommand = new RelayCommand(_ => FillFromCycleMenu());
            CopyPreviousMenuCommand = new RelayCommand(_ => StatusMessage = "Скопировано меню предыдущего дня");
            FillAttendanceByPlanCommand = new RelayCommand(_ => FillAttendanceByPlan());
            RecalculateTotalsCommand = new RelayCommand(_ => RecalculateTotals());
            AddDishCommand = new RelayCommand(_ => AddTestDish());
            ReplaceDishCommand = new RelayCommand(_ => ReplaceSelectedDish(), _ => SelectedDishItem != null);
            RemoveDishCommand = new RelayCommand(_ => RemoveSelectedDish(), _ => SelectedDishItem != null);
            CheckStockCommand = new RelayCommand(_ => CheckStock());
            OpenRelatedDocumentCommand = new RelayCommand(_ => StatusMessage = SelectedRelatedDocument == null ? "Документ не выбран" : $"Открыт документ: {SelectedRelatedDocument.DocumentTypeName} № {SelectedRelatedDocument.Number}");
            PrintRelatedDocumentCommand = new RelayCommand(_ => StatusMessage = SelectedRelatedDocument == null ? "Документ не выбран" : $"Печать документа: {SelectedRelatedDocument.DocumentTypeName} № {SelectedRelatedDocument.Number}");
            OpenRequirementCommand = new RelayCommand(_ => StatusMessage = "Открыта карточка меню-требования");
            CreateStockIssueCommand = new RelayCommand(_ => StatusMessage = "Создан документ выдачи со склада");
            OpenPrintPreviewCommand = new RelayCommand(_ => StatusMessage = "Открыта печатная форма");

            LoadDesignData();
        }

        /// <summary>
        /// Фабричный метод для использования в дизайнере XAML.
        /// </summary>
        public static DailyMenuEditorViewModel DesignInstance => new DailyMenuEditorViewModel();

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CalculateProductsCommand { get; }
        public ICommand CreateRequirementCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand FillFromCycleMenuCommand { get; }
        public ICommand CopyPreviousMenuCommand { get; }
        public ICommand FillAttendanceByPlanCommand { get; }
        public ICommand RecalculateTotalsCommand { get; }
        public ICommand AddDishCommand { get; }
        public ICommand ReplaceDishCommand { get; }
        public ICommand RemoveDishCommand { get; }
        public ICommand CheckStockCommand { get; }
        public ICommand OpenRelatedDocumentCommand { get; }
        public ICommand PrintRelatedDocumentCommand { get; }
        public ICommand OpenRequirementCommand { get; }
        public ICommand CreateStockIssueCommand { get; }
        public ICommand OpenPrintPreviewCommand { get; }

        public ObservableCollection<SeasonItemViewModel> Seasons { get; }
        public ObservableCollection<CycleMenuItemViewModel> CycleMenus { get; }
        public ObservableCollection<string> AvailableStatuses { get; }
        public ObservableCollection<AttendanceRowViewModel> Attendances { get; }
        public ObservableCollection<DishRowViewModel> Dishes { get; }
        public ObservableCollection<CalculatedProductRowViewModel> CalculatedProducts { get; }
        public ObservableCollection<RelatedDocumentRowViewModel> RelatedDocuments { get; }

        public DateTime? MenuDate
        {
            get => _menuDate;
            set
            {
                if (SetProperty(ref _menuDate, value))
                {
                    OnPropertyChanged(nameof(MenuDateText));
                }
            }
        }

        public string MenuDateText => MenuDate?.ToString("dd.MM.yyyy") ?? string.Empty;

        public SeasonItemViewModel? SelectedSeason
        {
            get => _selectedSeason;
            set => SetProperty(ref _selectedSeason, value);
        }

        public CycleMenuItemViewModel? SelectedCycleMenu
        {
            get => _selectedCycleMenu;
            set => SetProperty(ref _selectedCycleMenu, value);
        }

        public string? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    OnPropertyChanged(nameof(StatusName));
                }
            }
        }

        public string StatusName => SelectedStatus ?? string.Empty;

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string SourceDescription
        {
            get => _sourceDescription;
            set => SetProperty(ref _sourceDescription, value);
        }

        public string CreatedAtText
        {
            get => _createdAtText;
            set => SetProperty(ref _createdAtText, value);
        }

        public string UpdatedAtText
        {
            get => _updatedAtText;
            set => SetProperty(ref _updatedAtText, value);
        }

        public int TotalChildrenCount
        {
            get => _totalChildrenCount;
            set => SetProperty(ref _totalChildrenCount, value);
        }

        public int DishCount
        {
            get => _dishCount;
            set => SetProperty(ref _dishCount, value);
        }

        public bool HasDeficit
        {
            get => _hasDeficit;
            set => SetProperty(ref _hasDeficit, value);
        }

        public DishRowViewModel? SelectedDishItem
        {
            get => _selectedDishItem;
            set
            {
                if (SetProperty(ref _selectedDishItem, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public CalculatedProductRowViewModel? SelectedCalculatedProduct
        {
            get => _selectedCalculatedProduct;
            set => SetProperty(ref _selectedCalculatedProduct, value);
        }

        public RelatedDocumentRowViewModel? SelectedRelatedDocument
        {
            get => _selectedRelatedDocument;
            set => SetProperty(ref _selectedRelatedDocument, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public Brush AttendanceStatusBrush => Attendances.All(x => x.ActualCount > 0) ? Brushes.ForestGreen : Brushes.DarkGoldenrod;
        public Brush DishesStatusBrush => Dishes.Any() ? Brushes.ForestGreen : Brushes.IndianRed;
        public Brush CalculationStatusBrush => CalculatedProducts.Any() ? Brushes.ForestGreen : Brushes.IndianRed;
        public Brush RequirementStatusBrush => RelatedDocuments.Any(x => x.DocumentTypeName.Contains("Меню-требование")) ? Brushes.ForestGreen : Brushes.IndianRed;

        private void LoadDesignData()
        {
            MenuDate = new DateTime(2026, 3, 10);
            SourceDescription = "Заполнено из цикличного меню: Весеннее 10-дневное";
            CreatedAtText = "10.03.2026 07:30";
            UpdatedAtText = "10.03.2026 08:15";
            Notes = "Меню составлено с учетом фактической посещаемости и остатков на складе.";

            Seasons.Clear();
            Seasons.Add(new SeasonItemViewModel(1, "Зима"));
            Seasons.Add(new SeasonItemViewModel(2, "Весна"));
            Seasons.Add(new SeasonItemViewModel(3, "Лето"));
            Seasons.Add(new SeasonItemViewModel(4, "Осень"));
            SelectedSeason = Seasons.FirstOrDefault(x => x.Name == "Весна");

            CycleMenus.Clear();
            CycleMenus.Add(new CycleMenuItemViewModel(1, "Весеннее 10-дневное"));
            CycleMenus.Add(new CycleMenuItemViewModel(2, "Весеннее 14-дневное"));
            SelectedCycleMenu = CycleMenus.FirstOrDefault();

            AvailableStatuses.Clear();
            AvailableStatuses.Add("Черновик");
            AvailableStatuses.Add("Рассчитано");
            AvailableStatuses.Add("Утверждено");
            AvailableStatuses.Add("Закрыто");
            SelectedStatus = "Черновик";

            Attendances.Clear();
            Attendances.Add(new AttendanceRowViewModel("Младшая", "3-4 года", 25, 23, string.Empty));
            Attendances.Add(new AttendanceRowViewModel("Средняя", "4-5 лет", 22, 21, string.Empty));
            Attendances.Add(new AttendanceRowViewModel("Старшая", "5-6 лет", 20, 20, string.Empty));
            Attendances.Add(new AttendanceRowViewModel("Подготовительная", "6-7 лет", 21, 20, "2 ребенка отсутствуют"));

            Dishes.Clear();
            Dishes.Add(new DishRowViewModel("Завтрак", 1, "Каша манная молочная", "ТК-014", "200 г", string.Empty));
            Dishes.Add(new DishRowViewModel("Завтрак", 2, "Хлеб с маслом", "ТК-037", "35 г", string.Empty));
            Dishes.Add(new DishRowViewModel("Завтрак", 3, "Чай с сахаром", "ТК-052", "180 мл", string.Empty));
            Dishes.Add(new DishRowViewModel("Обед", 1, "Суп овощной", "ТК-101", "250 г", string.Empty));
            Dishes.Add(new DishRowViewModel("Обед", 2, "Котлета мясная", "ТК-215", "80 г", string.Empty));
            Dishes.Add(new DishRowViewModel("Обед", 3, "Компот из сухофруктов", "ТК-340", "180 мл", string.Empty));
            Dishes.Add(new DishRowViewModel("Полдник", 1, "Булочка сдобная", "ТК-451", "60 г", string.Empty));
            Dishes.Add(new DishRowViewModel("Полдник", 2, "Кефир", "ТК-501", "180 мл", string.Empty));
            SelectedDishItem = Dishes.FirstOrDefault();

            CalculatedProducts.Clear();
            CalculatedProducts.Add(new CalculatedProductRowViewModel("Молоко", "л", 18.50m, 12.00m, 6.50m, "Недостаточно на складе"));
            CalculatedProducts.Add(new CalculatedProductRowViewModel("Крупа манная", "кг", 2.10m, 10.00m, 0.00m, "В наличии"));
            CalculatedProducts.Add(new CalculatedProductRowViewModel("Масло сливочное", "кг", 1.20m, 0.70m, 0.50m, "Нужно пополнить остаток"));
            CalculatedProducts.Add(new CalculatedProductRowViewModel("Сахар", "кг", 0.95m, 5.00m, 0.00m, "В наличии"));

            RelatedDocuments.Clear();
            RelatedDocuments.Add(new RelatedDocumentRowViewModel("Меню-требование", "45", "10.03.2026", "Подготовлено", "Документ создан"));
            RelatedDocuments.Add(new RelatedDocumentRowViewModel("Выдача со склада", "17", "10.03.2026", "Черновик", "Ожидает проведения"));
            SelectedRelatedDocument = RelatedDocuments.FirstOrDefault();

            RecalculateTotals();
            RecalculateDerivedProperties();
            StatusMessage = "Дизайн-данные загружены";
        }

        private void FillFromCycleMenu()
        {
            StatusMessage = SelectedCycleMenu == null
                ? "Цикличное меню не выбрано"
                : $"Меню заполнено из шаблона: {SelectedCycleMenu.Name}";
        }

        private void FillAttendanceByPlan()
        {
            foreach (var row in Attendances)
            {
                row.ActualCount = row.PlannedCount;
            }

            RecalculateTotals();
            StatusMessage = "Фактическая посещаемость заполнена по плановой";
        }

        private void RecalculateTotals()
        {
            TotalChildrenCount = Attendances.Sum(x => x.ActualCount);
            DishCount = Dishes.Count;
            RecalculateDerivedProperties();
            StatusMessage = "Итоги пересчитаны";
        }

        private void RecalculateProducts()
        {
            foreach (var row in CalculatedProducts)
            {
                row.DeficitQuantity = Math.Max(0, row.RequiredQuantity - row.StockQuantity);
                row.StatusComment = row.DeficitQuantity > 0 ? "Недостаточно на складе" : "В наличии";
            }

            HasDeficit = CalculatedProducts.Any(x => x.DeficitQuantity > 0);
            SelectedStatus = "Рассчитано";
            RecalculateDerivedProperties();
            StatusMessage = HasDeficit ? "Расчет выполнен: обнаружен дефицит" : "Расчет выполнен успешно";
        }

        private void CheckStock()
        {
            HasDeficit = CalculatedProducts.Any(x => x.DeficitQuantity > 0);
            StatusMessage = HasDeficit ? "На складе недостаточно некоторых продуктов" : "Остатков достаточно";
        }

        private void AddTestDish()
        {
            var nextNumber = Dishes.Count(x => x.MealTypeName == "Полдник") + 1;
            var newItem = new DishRowViewModel("Полдник", nextNumber, "Яблоко свежее", "ТК-777", "100 г", "Тестовая позиция");
            Dishes.Add(newItem);
            SelectedDishItem = newItem;
            RecalculateTotals();
            StatusMessage = "Добавлено тестовое блюдо";
        }

        private void ReplaceSelectedDish()
        {
            if (SelectedDishItem == null)
                return;

            SelectedDishItem.DishName = $"{SelectedDishItem.DishName} (замена)";
            SelectedDishItem.Notes = "Блюдо заменено вручную";
            StatusMessage = "Выбранное блюдо заменено";
        }

        private void RemoveSelectedDish()
        {
            if (SelectedDishItem == null)
                return;

            var removedName = SelectedDishItem.DishName;
            Dishes.Remove(SelectedDishItem);
            SelectedDishItem = Dishes.FirstOrDefault();
            RenumberDishes();
            RecalculateTotals();
            StatusMessage = $"Удалено блюдо: {removedName}";
        }

        private void RenumberDishes()
        {
            foreach (var group in Dishes.GroupBy(x => x.MealTypeName))
            {
                var index = 1;
                foreach (var row in group)
                {
                    row.SortOrder = index++;
                }
            }
        }

        private void RecalculateDerivedProperties()
        {
            HasDeficit = CalculatedProducts.Any(x => x.DeficitQuantity > 0);
            OnPropertyChanged(nameof(AttendanceStatusBrush));
            OnPropertyChanged(nameof(DishesStatusBrush));
            OnPropertyChanged(nameof(CalculationStatusBrush));
            OnPropertyChanged(nameof(RequirementStatusBrush));
            OnPropertyChanged(nameof(StatusName));
            OnPropertyChanged(nameof(MenuDateText));
        }

        private void RaiseCommandStates()
        {
            (ReplaceDishCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveDishCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}

/*
Пример подключения дизайн-данных в XAML:

<UserControl ...
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:ProfitFood.UI.ViewModels"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:DailyMenuEditorViewModel, IsDesignTimeCreatable=True}">

Если нужен runtime DataContext без DI:

public partial class DailyMenuEditorView : UserControl
{
    public DailyMenuEditorView()
    {
        InitializeComponent();
        DataContext = new DailyMenuEditorViewModel();
    }
}
*/