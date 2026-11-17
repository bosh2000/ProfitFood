using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ProfitFood.UI.ViewModels.DashBoardViewModels
{
    /// <summary>
    /// Главная панель системы: состояние питания на сегодня из реальных данных.
    /// </summary>
    public sealed class DashboardViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IDailyMenuAppService _menuService;
        private readonly IStockAppService _stockService;
        private readonly IDocumentAppService _documentService;
        private readonly IProductAppService _productService;
        private readonly IChildGroupAppService _childGroupService;

        private DateTime _todayDate = DateTime.Today;
        private int _totalChildren;
        private int _dishCount;
        private int _productCount;
        private string _menuStatus = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;

        /// <summary>
        /// Инициализирует главную панель.
        /// </summary>
        /// <param name="menuService">Сервис меню: статус на сегодня и позиции.</param>
        /// <param name="stockService">Сервис остатков: позиции ниже минимума.</param>
        /// <param name="documentService">Сервис журнала документов.</param>
        /// <param name="productService">Сервис справочника продуктов.</param>
        /// <param name="childGroupService">Сервис групп детей: плановая численность.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public DashboardViewModel(
            IDailyMenuAppService menuService,
            IStockAppService stockService,
            IDocumentAppService documentService,
            IProductAppService productService,
            IChildGroupAppService childGroupService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _childGroupService = childGroupService ?? throw new ArgumentNullException(nameof(childGroupService));

            TodayMenuItems = new ObservableCollection<DashboardMenuItemViewModel>();
            CriticalStockProducts = new ObservableCollection<CriticalStockRowViewModel>();
            LastDocuments = new ObservableCollection<LastDocumentRowViewModel>();
            Warnings = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            CreateMenuCommand = new RelayCommand(_ => StatusMessage = "Создать меню можно в разделе «Меню».");
            OpenTodayMenuCommand = new RelayCommand(_ => StatusMessage = "Меню на сегодня открывается в разделе «Меню».");
            CalculateProductsCommand = new RelayCommand(_ => StatusMessage = "Расчёт продуктов ещё не реализован.");
            CreateRequirementCommand = new RelayCommand(_ => StatusMessage = "Меню-требование ещё не формируется: нужен расчёт продуктов.");
            PrintRequirementCommand = new RelayCommand(_ => StatusMessage = "Печать документов ещё не реализована.");
        }

        /// <summary>Команды быстрых действий.</summary>
        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateMenuCommand { get; }
        public RelayCommand OpenTodayMenuCommand { get; }
        public RelayCommand CalculateProductsCommand { get; }
        public RelayCommand CreateRequirementCommand { get; }
        public RelayCommand PrintRequirementCommand { get; }

        /// <summary>Позиции меню на сегодня.</summary>
        public ObservableCollection<DashboardMenuItemViewModel> TodayMenuItems { get; }

        /// <summary>Продукты с остатком ниже минимального.</summary>
        public ObservableCollection<CriticalStockRowViewModel> CriticalStockProducts { get; }

        /// <summary>Последние складские документы.</summary>
        public ObservableCollection<LastDocumentRowViewModel> LastDocuments { get; }

        /// <summary>Предупреждения, собранные из фактического состояния данных.</summary>
        public ObservableCollection<string> Warnings { get; }

        /// <summary>Дата, на которую показывается состояние.</summary>
        public DateTime TodayDate
        {
            get => _todayDate;
            private set
            {
                if (SetProperty(ref _todayDate, value))
                {
                    OnPropertyChanged(nameof(TodayDateText));
                }
            }
        }

        /// <summary>Дата в формате dd.MM.yyyy.</summary>
        public string TodayDateText => TodayDate.ToString("dd.MM.yyyy");

        /// <summary>Плановая численность детей по всем группам.</summary>
        public int TotalChildren
        {
            get => _totalChildren;
            private set => SetProperty(ref _totalChildren, value);
        }

        /// <summary>Количество блюд в меню на сегодня.</summary>
        public int DishCount
        {
            get => _dishCount;
            private set => SetProperty(ref _dishCount, value);
        }

        /// <summary>Количество продуктов в справочнике.</summary>
        public int ProductCount
        {
            get => _productCount;
            private set => SetProperty(ref _productCount, value);
        }

        /// <summary>Статус меню на сегодня.</summary>
        public string MenuStatus
        {
            get => _menuStatus;
            private set
            {
                if (SetProperty(ref _menuStatus, value))
                {
                    OnPropertyChanged(nameof(MenuStatusBrush));
                }
            }
        }

        /// <summary>Цвет индикатора статуса меню.</summary>
        public Brush MenuStatusBrush => MenuStatus switch
        {
            "Рассчитано" => Brushes.ForestGreen,
            "Утверждено" => Brushes.ForestGreen,
            "Черновик" => Brushes.DarkGoldenrod,
            "Закрыто" => Brushes.SlateGray,
            "Нет меню" => Brushes.IndianRed,
            _ => Brushes.SteelBlue
        };

        /// <summary>Сообщение о состоянии загрузки.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>Признак выполняющейся загрузки.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    RefreshCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>Загружает данные при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Собирает состояние питания на сегодня из базы.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                var today = DateOnly.FromDateTime(DateTime.Today);
                TodayDate = DateTime.Today;

                var warnings = new List<string>();

                var products = await _productService.GetAllAsync();
                ProductCount = products.Count;

                var groups = await _childGroupService.GetAllAsync();
                TotalChildren = groups.Sum(x => x.PlannedChildrenCount);

                var balances = await _stockService.GetAllAsync();
                FillCriticalStock(balances);

                var documents = await _documentService.GetAllAsync();
                FillLastDocuments(documents);

                var menus = await _menuService.GetAllAsync();
                var todayMenu = menus.FirstOrDefault(x => x.MenuDate == today);

                if (todayMenu is null)
                {
                    MenuStatus = "Нет меню";
                    TodayMenuItems.Clear();
                    DishCount = 0;
                    warnings.Add("На сегодня меню не создано.");
                }
                else
                {
                    MenuStatus = MapMenuStatus(todayMenu.StatusName);

                    var items = await _menuService.GetItemsAsync(todayMenu.Id);

                    TodayMenuItems.Clear();
                    foreach (var item in items)
                    {
                        TodayMenuItems.Add(new DashboardMenuItemViewModel(
                            item.MealTypeName,
                            item.DishName,
                            item.RecipeCardNumber,
                            FormatOutput(item.OutputQuantity, item.OutputUnitName)));
                    }

                    DishCount = items.Count;

                    if (items.Count == 0)
                    {
                        warnings.Add("В меню на сегодня не добавлено ни одной позиции.");
                    }
                }

                var deficitCount = balances.Count(x => x.IsDeficit);
                if (deficitCount > 0)
                {
                    warnings.Add($"Ниже минимального остатка: {deficitCount} позиц.");
                }

                var unpostedToday = documents.Count(x => x.StatusName == nameof(DocumentStatus.Draft) && x.DocumentDate == today);
                if (unpostedToday > 0)
                {
                    warnings.Add($"Не проведено документов за сегодня: {unpostedToday}.");
                }

                Warnings.Clear();
                foreach (var warning in warnings)
                {
                    Warnings.Add(warning);
                }

                StatusMessage = $"Данные обновлены в {DateTime.Now:HH:mm}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки данных: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Заполняет блок критичных остатков, ограничивая список десятью строками.</summary>
        private void FillCriticalStock(IReadOnlyCollection<ProfitFood.Applications.Dto.Stocks.StockBalanceDto> balances)
        {
            CriticalStockProducts.Clear();

            foreach (var balance in balances.Where(x => x.IsDeficit).Take(10))
            {
                CriticalStockProducts.Add(new CriticalStockRowViewModel(
                    balance.ProductName,
                    $"{balance.Quantity:0.##} {balance.UnitName}".Trim(),
                    $"{balance.MinStock:0.##} {balance.UnitName}".Trim()));
            }
        }

        /// <summary>Заполняет блок последних документов пятью самыми свежими записями.</summary>
        private void FillLastDocuments(IReadOnlyCollection<ProfitFood.Applications.Dto.Documents.StockDocumentListItemDto> documents)
        {
            LastDocuments.Clear();

            foreach (var document in documents.Take(5))
            {
                LastDocuments.Add(new LastDocumentRowViewModel(
                    $"{document.DocumentNumber} — {document.DocumentTypeName}",
                    document.DocumentDate.ToString("dd.MM.yyyy"),
                    document.StatusName));
            }
        }

        /// <summary>Переводит имя статуса меню в русскую подпись.</summary>
        private static string MapMenuStatus(string statusName) => statusName switch
        {
            nameof(DailyMenuStatus.Draft) => "Черновик",
            nameof(DailyMenuStatus.Calculated) => "Рассчитано",
            nameof(DailyMenuStatus.Approved) => "Утверждено",
            nameof(DailyMenuStatus.Closed) => "Закрыто",
            _ => statusName
        };

        /// <summary>Форматирует выход порции; при отсутствии данных возвращает прочерк.</summary>
        private static string FormatOutput(decimal? quantity, string unitName)
            => quantity.HasValue
                ? $"{quantity.Value:0.##} {unitName}".Trim()
                : "—";
    }
}
