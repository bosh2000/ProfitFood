using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Dto.Stocks;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Остатки продуктов на складах с возможностью внести и изменить запись.
    /// </summary>
    public sealed class StockViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IStockAppService _service;
        private readonly IProductAppService _productService;
        private readonly IStorageLocationAppService _locationService;
        private readonly IUnitAppService _unitService;

        private StockBalanceDto? _selectedItem;
        private StockBalanceDto _editModel = new();
        private string _searchText = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;
        private bool _isEditMode;
        private bool _isCreateMode;

        /// <summary>
        /// Инициализирует список складских остатков.
        /// </summary>
        /// <param name="service">Сервис остатков.</param>
        /// <param name="productService">Сервис справочника продуктов для выпадающего списка.</param>
        /// <param name="locationService">Сервис справочника складов для выпадающего списка.</param>
        /// <param name="unitService">Сервис справочника единиц измерения для выпадающего списка.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public StockViewModel(
            IStockAppService service,
            IProductAppService productService,
            IStorageLocationAppService locationService,
            IUnitAppService unitService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

            Items = new ObservableCollection<StockBalanceDto>();
            ProductOptions = new ObservableCollection<ProductDto>();
            StorageLocationOptions = new ObservableCollection<StorageLocationDto>();
            UnitOptions = new ObservableCollection<UnitItemDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            CreateCommand = new RelayCommand(_ => StartCreate(), _ => !IsBusy);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => Cancel(), _ => !IsBusy);
        }

        /// <summary>Остатки по складам.</summary>
        public ObservableCollection<StockBalanceDto> Items { get; }

        /// <summary>Продукты, доступные для выбора в карточке.</summary>
        public ObservableCollection<ProductDto> ProductOptions { get; }

        /// <summary>Склады, доступные для выбора в карточке.</summary>
        public ObservableCollection<StorageLocationDto> StorageLocationOptions { get; }

        /// <summary>Единицы измерения, доступные для выбора в карточке.</summary>
        public ObservableCollection<UnitItemDto> UnitOptions { get; }

        /// <summary>Заголовок раздела.</summary>
        public string Title => "Склад";

        /// <summary>Выбранная строка остатка.</summary>
        public StockBalanceDto? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value is not null)
                {
                    EditModel = BuildEditModel(value);
                    IsCreateMode = false;
                    IsEditMode = true;
                    StatusMessage = "Запись выбрана для редактирования.";
                }

                UpdateCommandState();
            }
        }

        /// <summary>Модель, привязанная к карточке редактирования.</summary>
        public StockBalanceDto EditModel
        {
            get => _editModel;
            private set => SetProperty(ref _editModel, value);
        }

        /// <summary>Текст поиска по продукту или складу.</summary>
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        /// <summary>Сообщение о состоянии загрузки.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>Признак выполняющейся операции.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    UpdateCommandState();
                }
            }
        }

        /// <summary>Признак того, что карточка редактирует существующую запись.</summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            private set => SetProperty(ref _isEditMode, value);
        }

        /// <summary>Признак того, что карточка создаёт новую запись.</summary>
        public bool IsCreateMode
        {
            get => _isCreateMode;
            private set => SetProperty(ref _isCreateMode, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Загружает остатки при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Загружает все остатки и справочники для выпадающих списков.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadOptionsAsync();
                ReplaceItems(await _service.GetAllAsync());

                // Дефицитные позиции выделяем в статусе: это главное, что ищут на этом экране.
                var deficitCount = Items.Count(x => x.IsDeficit);
                StatusMessage = $"Позиций: {Items.Count}, из них ниже минимума: {deficitCount}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Сохраняет остаток и перезагружает список.</summary>
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                await _service.SaveAsync(EditModel);
                await LoadAsync();

                StartCreate();
                StatusMessage = "Остаток сохранён.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка сохранения: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Ищет остатки по введённому тексту.</summary>
        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;
                ReplaceItems(await _service.SearchAsync(SearchText));
                StatusMessage = $"Найдено позиций: {Items.Count}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка поиска: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Сбрасывает карточку в режим создания новой записи.</summary>
        private void StartCreate()
        {
            SelectedItem = null;
            EditModel = new StockBalanceDto();
            IsCreateMode = true;
            IsEditMode = false;
        }

        /// <summary>Отменяет редактирование и возвращает карточку в исходное состояние.</summary>
        private void Cancel()
        {
            StartCreate();
            StatusMessage = "Редактирование отменено.";
        }

        /// <summary>
        /// Перезаполняет справочники продуктов, складов и единиц для карточки.
        /// </summary>
        private async Task LoadOptionsAsync()
        {
            var products = await _productService.GetAllAsync();
            var locations = await _locationService.GetAllAsync();
            var units = await _unitService.GetAllAsync();

            ProductOptions.Clear();
            foreach (var product in products)
            {
                ProductOptions.Add(product);
            }

            StorageLocationOptions.Clear();
            foreach (var location in locations)
            {
                StorageLocationOptions.Add(location);
            }

            UnitOptions.Clear();
            foreach (var unit in units)
            {
                UnitOptions.Add(unit);
            }
        }

        private void ReplaceItems(IEnumerable<StockBalanceDto> items)
        {
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private static StockBalanceDto BuildEditModel(StockBalanceDto item) => new()
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            StorageLocationId = item.StorageLocationId,
            StorageLocationName = item.StorageLocationName,
            UnitId = item.UnitId,
            UnitName = item.UnitName,
            Quantity = item.Quantity,
            MinStock = item.MinStock,
            IsDeficit = item.IsDeficit
        };

        private void UpdateCommandState()
        {
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CreateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
