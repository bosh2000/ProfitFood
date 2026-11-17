using ProfitFood.Applications.Dto.Documents;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Журнал складских документов с формой заполнения и проведением.
    /// </summary>
    public sealed class DocumentsViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IDocumentAppService _service;
        private readonly IProductAppService _productService;
        private readonly IUnitAppService _unitService;
        private readonly IStorageLocationAppService _locationService;

        private StockDocumentListItemDto? _selectedItem;
        private StockDocumentDto _editModel = new();
        private StockDocumentItemDto? _selectedDocumentItem;
        private DateTime? _editDate = DateTime.Today;
        private Guid? _newItemProductId;
        private Guid? _newItemUnitId;
        private decimal _newItemQuantity = 1m;
        private decimal _newItemPrice;
        private string _searchText = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;

        /// <summary>
        /// Инициализирует раздел документов.
        /// </summary>
        /// <param name="service">Сервис документов.</param>
        /// <param name="productService">Сервис справочника продуктов.</param>
        /// <param name="unitService">Сервис справочника единиц измерения.</param>
        /// <param name="locationService">Сервис справочника складов.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public DocumentsViewModel(
            IDocumentAppService service,
            IProductAppService productService,
            IUnitAppService unitService,
            IStorageLocationAppService locationService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));

            Items = new ObservableCollection<StockDocumentListItemDto>();
            DocumentItems = new ObservableCollection<StockDocumentItemDto>();
            ProductOptions = new ObservableCollection<ProductDto>();
            UnitOptions = new ObservableCollection<UnitItemDto>();
            StorageLocationOptions = new ObservableCollection<StorageLocationDto>();

            DocumentTypes = new ReadOnlyCollection<DocumentTypeOption>(new List<DocumentTypeOption>
            {
                new(StockDocumentType.Receipt, "Приход"),
                new(StockDocumentType.Issue, "Выдача"),
                new(StockDocumentType.WriteOff, "Списание"),
                new(StockDocumentType.Inventory, "Инвентаризация")
            });

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            CreateDocumentCommand = new RelayCommand(_ => StartCreate(), _ => !IsBusy);
            SaveDocumentCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy && IsDraft);
            PostCommand = new RelayCommand(async _ => await PostAsync(), _ => !IsBusy && IsDraft);
            UnpostCommand = new RelayCommand(async _ => await UnpostAsync(), _ => !IsBusy && IsPosted);
            DeleteDocumentCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && IsDraft);
            AddItemCommand = new RelayCommand(_ => AddItem(), _ => !IsBusy && IsDraft);
            RemoveItemCommand = new RelayCommand(_ => RemoveItem(), _ => !IsBusy && IsDraft && SelectedDocumentItem is not null);
        }

        /// <summary>Документы журнала, начиная с самого свежего.</summary>
        public ObservableCollection<StockDocumentListItemDto> Items { get; }

        /// <summary>Строки редактируемого документа.</summary>
        public ObservableCollection<StockDocumentItemDto> DocumentItems { get; }

        /// <summary>Продукты для выбора в строках документа.</summary>
        public ObservableCollection<ProductDto> ProductOptions { get; }

        /// <summary>Единицы измерения для выбора в строках документа.</summary>
        public ObservableCollection<UnitItemDto> UnitOptions { get; }

        /// <summary>Склады для выбора в шапке документа.</summary>
        public ObservableCollection<StorageLocationDto> StorageLocationOptions { get; }

        /// <summary>Доступные типы документов с русскими подписями.</summary>
        public IReadOnlyList<DocumentTypeOption> DocumentTypes { get; }

        /// <summary>Заголовок раздела.</summary>
        public string Title => "Документы";

        /// <summary>Признак того, что документ можно править.</summary>
        public bool IsDraft => EditModel.Status == DocumentStatus.Draft;

        /// <summary>Признак того, что документ проведён.</summary>
        public bool IsPosted => EditModel.Status == DocumentStatus.Posted;

        /// <summary>Выбранный документ журнала.</summary>
        public StockDocumentListItemDto? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value is not null)
                {
                    _ = LoadDocumentAsync(value.Id);
                }
            }
        }

        /// <summary>Документ в карточке заполнения.</summary>
        public StockDocumentDto EditModel
        {
            get => _editModel;
            private set
            {
                if (SetProperty(ref _editModel, value))
                {
                    OnPropertyChanged(nameof(IsDraft));
                    OnPropertyChanged(nameof(IsPosted));
                    UpdateCommandState();
                }
            }
        }

        /// <summary>Выбранная строка документа.</summary>
        public StockDocumentItemDto? SelectedDocumentItem
        {
            get => _selectedDocumentItem;
            set
            {
                if (SetProperty(ref _selectedDocumentItem, value))
                {
                    UpdateCommandState();
                }
            }
        }

        /// <summary>
        /// Дата документа для DatePicker: DateOnly нельзя привязать к элементу напрямую,
        /// поэтому в форме живёт DateTime.
        /// </summary>
        public DateTime? EditDate
        {
            get => _editDate;
            set => SetProperty(ref _editDate, value);
        }

        /// <summary>Продукт для добавляемой строки.</summary>
        public Guid? NewItemProductId
        {
            get => _newItemProductId;
            set => SetProperty(ref _newItemProductId, value);
        }

        /// <summary>Единица измерения для добавляемой строки.</summary>
        public Guid? NewItemUnitId
        {
            get => _newItemUnitId;
            set => SetProperty(ref _newItemUnitId, value);
        }

        /// <summary>Количество для добавляемой строки.</summary>
        public decimal NewItemQuantity
        {
            get => _newItemQuantity;
            set => SetProperty(ref _newItemQuantity, value);
        }

        /// <summary>Цена для добавляемой строки.</summary>
        public decimal NewItemPrice
        {
            get => _newItemPrice;
            set => SetProperty(ref _newItemPrice, value);
        }

        /// <summary>Текст поиска по номеру документа.</summary>
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        /// <summary>Сообщение о состоянии работы.</summary>
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

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateDocumentCommand { get; }
        public ICommand SaveDocumentCommand { get; }
        public ICommand PostCommand { get; }
        public ICommand UnpostCommand { get; }
        public ICommand DeleteDocumentCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        /// <summary>Загружает журнал при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Загружает журнал документов и справочники для формы.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadOptionsAsync();

                ReplaceItems(await _service.GetAllAsync());
                StatusMessage = $"Загружено документов: {Items.Count}.";
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

        /// <summary>Загружает документ со строками в карточку.</summary>
        private async Task LoadDocumentAsync(Guid id)
        {
            try
            {
                IsBusy = true;

                var document = await _service.GetByIdAsync(id);

                if (document is null)
                {
                    StatusMessage = "Документ не найден: возможно, его удалили.";
                    return;
                }

                ApplyDocument(document);
                StatusMessage = document.IsPosted
                    ? "Документ проведён: правка недоступна, можно отменить проведение."
                    : "Документ открыт для правки.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки документа: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Сохраняет документ вместе со строками.</summary>
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                SyncEditModel();

                var id = await _service.SaveAsync(EditModel);
                await LoadAsync();

                var document = await _service.GetByIdAsync(id);
                if (document is not null)
                {
                    ApplyDocument(document);
                }

                StatusMessage = $"Документ {EditModel.DocumentNumber} сохранён.";
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

        /// <summary>Проводит документ с движением товара.</summary>
        private async Task PostAsync()
        {
            try
            {
                IsBusy = true;

                await _service.PostAsync(EditModel);
                await ReloadDocumentAsync();

                StatusMessage = "Документ проведён: остатки обновлены.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка проведения: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Отменяет проведение документа.</summary>
        private async Task UnpostAsync()
        {
            try
            {
                IsBusy = true;

                await _service.UnpostAsync(EditModel);
                await ReloadDocumentAsync();

                StatusMessage = "Проведение отменено: остатки возвращены.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка отмены проведения: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Удаляет документ.</summary>
        private async Task DeleteAsync()
        {
            try
            {
                IsBusy = true;

                await _service.DeleteAsync(EditModel);
                StartCreate();

                await LoadAsync();
                StatusMessage = "Документ удалён.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка удаления: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Ищет документы по номеру.</summary>
        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;
                ReplaceItems(await _service.SearchAsync(SearchText));
                StatusMessage = $"Найдено документов: {Items.Count}.";
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

        /// <summary>Добавляет строку в документ из значений панели.</summary>
        private void AddItem()
        {
            if (NewItemProductId is null || NewItemProductId == Guid.Empty)
            {
                StatusMessage = "Выберите продукт для строки документа.";
                return;
            }

            if (NewItemUnitId is null || NewItemUnitId == Guid.Empty)
            {
                StatusMessage = "Выберите единицу измерения для строки документа.";
                return;
            }

            if (NewItemQuantity <= 0m)
            {
                StatusMessage = "Количество должно быть больше нуля.";
                return;
            }

            var product = ProductOptions.FirstOrDefault(x => x.Id == NewItemProductId.Value);
            var unit = UnitOptions.FirstOrDefault(x => x.Id == NewItemUnitId.Value);

            DocumentItems.Add(new StockDocumentItemDto
            {
                ProductId = NewItemProductId.Value,
                ProductName = product?.Name ?? string.Empty,
                UnitId = NewItemUnitId.Value,
                UnitName = unit?.Name ?? string.Empty,
                Quantity = NewItemQuantity,
                Price = NewItemPrice,
                Amount = NewItemQuantity * NewItemPrice
            });

            NewItemQuantity = 1m;
            NewItemPrice = 0m;
            StatusMessage = "Строка добавлена.";
        }

        /// <summary>Удаляет выбранную строку документа.</summary>
        private void RemoveItem()
        {
            if (SelectedDocumentItem is null)
            {
                return;
            }

            DocumentItems.Remove(SelectedDocumentItem);
            SelectedDocumentItem = null;
            StatusMessage = "Строка удалена.";
        }

        /// <summary>Сбрасывает карточку в режим создания нового документа.</summary>
        private void StartCreate()
        {
            SelectedItem = null;
            ApplyDocument(new StockDocumentDto
            {
                DocumentType = StockDocumentType.Receipt,
                DocumentDate = DateOnly.FromDateTime(DateTime.Today),
                Status = DocumentStatus.Draft
            });

            EditDate = DateTime.Today;
        }

        /// <summary>Переносит документ в карточку и строки.</summary>
        private void ApplyDocument(StockDocumentDto document)
        {
            EditModel = document;
            EditDate = document.DocumentDate.ToDateTime(TimeOnly.MinValue);

            DocumentItems.Clear();
            foreach (var item in document.Items)
            {
                DocumentItems.Add(item);
            }

            SelectedDocumentItem = null;
        }

        /// <summary>Перечитывает открытый документ после проведения или отмены.</summary>
        private async Task ReloadDocumentAsync()
        {
            if (EditModel.Id == Guid.Empty)
            {
                return;
            }

            var document = await _service.GetByIdAsync(EditModel.Id);
            if (document is not null)
            {
                ApplyDocument(document);
            }
        }

        /// <summary>Собирает карточку из состояния формы перед сохранением.</summary>
        private void SyncEditModel()
        {
            EditModel.DocumentDate = EditDate.HasValue
                ? DateOnly.FromDateTime(EditDate.Value)
                : DateOnly.FromDateTime(DateTime.Today);

            EditModel.Items = DocumentItems.ToList();
        }

        /// <summary>Перезаполняет справочники и список типов документов.</summary>
        private async Task LoadOptionsAsync()
        {
            var products = await _productService.GetAllAsync();
            var units = await _unitService.GetAllAsync();
            var locations = await _locationService.GetAllAsync();

            ProductOptions.Clear();
            foreach (var product in products)
            {
                ProductOptions.Add(product);
            }

            UnitOptions.Clear();
            foreach (var unit in units)
            {
                UnitOptions.Add(unit);
            }

            StorageLocationOptions.Clear();
            foreach (var location in locations)
            {
                StorageLocationOptions.Add(location);
            }
        }

        private void ReplaceItems(IEnumerable<StockDocumentListItemDto> items)
        {
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private void UpdateCommandState()
        {
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CreateDocumentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveDocumentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PostCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (UnpostCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteDocumentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>Тип документа с русской подписью для выпадающего списка.</summary>
        /// <param name="Value">Значение перечисления.</param>
        /// <param name="Title">Подпись в интерфейсе.</param>
        public sealed record DocumentTypeOption(StockDocumentType Value, string Title);
    }
}
