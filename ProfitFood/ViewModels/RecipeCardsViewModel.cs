using ProfitFood.Applications.Dto.Recipes;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Журнал технологических карт с формой заполнения версии и ингредиентов.
    /// </summary>
    public sealed class RecipeCardsViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IRecipeCardAppService _service;
        private readonly IDishAppService _dishService;
        private readonly IUnitAppService _unitService;
        private readonly IProductAppService _productService;

        private RecipeCardListItemDto? _selectedItem;
        private RecipeCardDto _editModel = new();
        private RecipeIngredientDto? _selectedIngredient;
        private DateTime? _versionDateFrom = DateTime.Today;
        private DateTime? _versionDateTo;
        private Guid? _newIngredientProductId;
        private Guid? _newIngredientUnitId;
        private decimal _newGrossQuantity = 1m;
        private decimal _newNetQuantity = 1m;
        private decimal _newLossPercent;
        private string _searchText = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;

        /// <summary>
        /// Инициализирует раздел техкарт.
        /// </summary>
        /// <param name="service">Сервис технологических карт.</param>
        /// <param name="dishService">Сервис справочника блюд.</param>
        /// <param name="unitService">Сервис справочника единиц измерения.</param>
        /// <param name="productService">Сервис справочника продуктов.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public RecipeCardsViewModel(
            IRecipeCardAppService service,
            IDishAppService dishService,
            IUnitAppService unitService,
            IProductAppService productService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

            Items = new ObservableCollection<RecipeCardListItemDto>();
            Ingredients = new ObservableCollection<RecipeIngredientDto>();
            DishOptions = new ObservableCollection<DishDto>();
            UnitOptions = new ObservableCollection<UnitItemDto>();
            ProductOptions = new ObservableCollection<ProductDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            CreateCommand = new RelayCommand(_ => StartCreate(), _ => !IsBusy);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && EditModel.Id != Guid.Empty);
            AddIngredientCommand = new RelayCommand(_ => AddIngredient(), _ => !IsBusy);
            RemoveIngredientCommand = new RelayCommand(_ => RemoveIngredient(), _ => !IsBusy && SelectedIngredient is not null);
        }

        /// <summary>Техкарты в порядке номера.</summary>
        public ObservableCollection<RecipeCardListItemDto> Items { get; }

        /// <summary>Ингредиенты редактируемой версии.</summary>
        public ObservableCollection<RecipeIngredientDto> Ingredients { get; }

        /// <summary>Блюда для выбора в карточке.</summary>
        public ObservableCollection<DishDto> DishOptions { get; }

        /// <summary>Единицы измерения для выхода и ингредиентов.</summary>
        public ObservableCollection<UnitItemDto> UnitOptions { get; }

        /// <summary>Продукты для выбора в строках ингредиентов.</summary>
        public ObservableCollection<ProductDto> ProductOptions { get; }

        /// <summary>Заголовок раздела.</summary>
        public string Title => "Техкарты";

        /// <summary>Выбранная техкарта в журнале.</summary>
        public RecipeCardListItemDto? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value is not null)
                {
                    _ = LoadCardAsync(value.Id);
                }
            }
        }

        /// <summary>Техкарта в карточке заполнения.</summary>
        public RecipeCardDto EditModel
        {
            get => _editModel;
            private set
            {
                if (SetProperty(ref _editModel, value))
                {
                    OnPropertyChanged(nameof(VersionNumber));
                    OnPropertyChanged(nameof(OutputQuantity));
                    OnPropertyChanged(nameof(OutputUnitId));
                    OnPropertyChanged(nameof(IsApproved));
                    OnPropertyChanged(nameof(VersionDescription));
                    UpdateCommandState();
                }
            }
        }

        /// <summary>Номер версии (только для отображения).</summary>
        public int VersionNumber => EditModel.Version.VersionNumber;

        /// <summary>Выход блюда в единице выхода.</summary>
        public decimal OutputQuantity
        {
            get => EditModel.Version.OutputQuantity;
            set
            {
                if (EditModel.Version.OutputQuantity == value)
                {
                    return;
                }

                EditModel.Version.OutputQuantity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Единица измерения выхода.</summary>
        public Guid OutputUnitId
        {
            get => EditModel.Version.OutputUnitId;
            set
            {
                if (EditModel.Version.OutputUnitId == value)
                {
                    return;
                }

                EditModel.Version.OutputUnitId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Признак утверждения версии.</summary>
        public bool IsApproved
        {
            get => EditModel.Version.IsApproved;
            set
            {
                if (EditModel.Version.IsApproved == value)
                {
                    return;
                }

                EditModel.Version.IsApproved = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Описание версии.</summary>
        public string? VersionDescription
        {
            get => EditModel.Version.Description;
            set
            {
                if (EditModel.Version.Description == value)
                {
                    return;
                }

                EditModel.Version.Description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Дата начала действия версии для DatePicker: DateOnly нельзя привязать напрямую.
        /// </summary>
        public DateTime? VersionDateFrom
        {
            get => _versionDateFrom;
            set => SetProperty(ref _versionDateFrom, value);
        }

        /// <summary>Дата окончания действия версии; пустая означает «бессрочно».</summary>
        public DateTime? VersionDateTo
        {
            get => _versionDateTo;
            set => SetProperty(ref _versionDateTo, value);
        }

        /// <summary>Выбранный ингредиент.</summary>
        public RecipeIngredientDto? SelectedIngredient
        {
            get => _selectedIngredient;
            set
            {
                if (SetProperty(ref _selectedIngredient, value))
                {
                    UpdateCommandState();
                }
            }
        }

        /// <summary>Продукт для добавляемой строки ингредиента.</summary>
        public Guid? NewIngredientProductId
        {
            get => _newIngredientProductId;
            set => SetProperty(ref _newIngredientProductId, value);
        }

        /// <summary>Единица измерения для добавляемой строки ингредиента.</summary>
        public Guid? NewIngredientUnitId
        {
            get => _newIngredientUnitId;
            set => SetProperty(ref _newIngredientUnitId, value);
        }

        /// <summary>Количество брутто для добавляемой строки.</summary>
        public decimal NewGrossQuantity
        {
            get => _newGrossQuantity;
            set => SetProperty(ref _newGrossQuantity, value);
        }

        /// <summary>Количество нетто для добавляемой строки.</summary>
        public decimal NewNetQuantity
        {
            get => _newNetQuantity;
            set => SetProperty(ref _newNetQuantity, value);
        }

        /// <summary>Процент потерь для добавляемой строки.</summary>
        public decimal NewLossPercent
        {
            get => _newLossPercent;
            set => SetProperty(ref _newLossPercent, value);
        }

        /// <summary>Текст поиска по номеру или наименованию.</summary>
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
        public ICommand CreateCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddIngredientCommand { get; }
        public ICommand RemoveIngredientCommand { get; }

        /// <summary>Загружает журнал при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Загружает техкарты и справочники для карточки.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadOptionsAsync();

                ReplaceItems(await _service.GetAllAsync());
                StatusMessage = $"Загружено техкарт: {Items.Count}.";
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

        /// <summary>Загружает техкарту в карточку.</summary>
        private async Task LoadCardAsync(Guid id)
        {
            try
            {
                IsBusy = true;

                var card = await _service.GetByIdAsync(id);

                if (card is null)
                {
                    StatusMessage = "Техкарта не найдена: возможно, её удалили.";
                    return;
                }

                ApplyCard(card);
                StatusMessage = "Техкарта открыта для правки.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки техкарты: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Сохраняет техкарту вместе с версией и ингредиентами.</summary>
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                SyncEditModel();

                var id = await _service.SaveAsync(EditModel);
                await LoadAsync();

                var card = await _service.GetByIdAsync(id);
                if (card is not null)
                {
                    ApplyCard(card);
                }

                StatusMessage = $"Техкарта {EditModel.CardNumber} сохранена.";
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

        /// <summary>Удаляет техкарту.</summary>
        private async Task DeleteAsync()
        {
            try
            {
                IsBusy = true;

                await _service.DeleteAsync(EditModel);
                StartCreate();

                await LoadAsync();
                StatusMessage = "Техкарта удалена.";
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

        /// <summary>Ищет техкарты по введённому тексту.</summary>
        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;
                ReplaceItems(await _service.SearchAsync(SearchText));
                StatusMessage = $"Найдено техкарт: {Items.Count}.";
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

        /// <summary>Добавляет строку ингредиента из значений панели.</summary>
        private void AddIngredient()
        {
            if (NewIngredientProductId is null || NewIngredientProductId == Guid.Empty)
            {
                StatusMessage = "Выберите продукт для строки ингредиента.";
                return;
            }

            if (NewIngredientUnitId is null || NewIngredientUnitId == Guid.Empty)
            {
                StatusMessage = "Выберите единицу измерения для строки ингредиента.";
                return;
            }

            if (NewNetQuantity <= 0m)
            {
                StatusMessage = "Количество нетто должно быть больше нуля.";
                return;
            }

            var product = ProductOptions.FirstOrDefault(x => x.Id == NewIngredientProductId.Value);
            var unit = UnitOptions.FirstOrDefault(x => x.Id == NewIngredientUnitId.Value);

            Ingredients.Add(new RecipeIngredientDto
            {
                ProductId = NewIngredientProductId.Value,
                ProductName = product?.Name ?? string.Empty,
                UnitId = NewIngredientUnitId.Value,
                UnitName = unit?.Name ?? string.Empty,
                GrossQuantity = NewGrossQuantity,
                NetQuantity = NewNetQuantity,
                LossPercent = NewLossPercent,
                SortOrder = Ingredients.Count + 1
            });

            NewGrossQuantity = 1m;
            NewNetQuantity = 1m;
            NewLossPercent = 0m;
            StatusMessage = "Ингредиент добавлен.";
        }

        /// <summary>Удаляет выбранную строку ингредиента.</summary>
        private void RemoveIngredient()
        {
            if (SelectedIngredient is null)
            {
                return;
            }

            Ingredients.Remove(SelectedIngredient);
            SelectedIngredient = null;
            StatusMessage = "Ингредиент удалён.";
        }

        /// <summary>Сбрасывает карточку в режим создания новой техкарты.</summary>
        private void StartCreate()
        {
            SelectedItem = null;
            ApplyCard(new RecipeCardDto
            {
                Version = new RecipeCardVersionDto
                {
                    VersionNumber = 1,
                    DateFrom = DateOnly.FromDateTime(DateTime.Today),
                    OutputQuantity = 0m
                }
            });

            VersionDateFrom = DateTime.Today;
            VersionDateTo = null;
        }

        /// <summary>Переносит техкарту в карточку и строки ингредиентов.</summary>
        private void ApplyCard(RecipeCardDto card)
        {
            EditModel = card;

            VersionDateFrom = card.Version.DateFrom == default
                ? DateTime.Today
                : card.Version.DateFrom.ToDateTime(TimeOnly.MinValue);

            VersionDateTo = card.Version.DateTo.HasValue
                ? card.Version.DateTo.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            Ingredients.Clear();
            foreach (var ingredient in card.Version.Ingredients)
            {
                Ingredients.Add(ingredient);
            }

            SelectedIngredient = null;
        }

        /// <summary>Собирает карточку из состояния формы перед сохранением.</summary>
        private void SyncEditModel()
        {
            EditModel.Version.DateFrom = VersionDateFrom.HasValue
                ? DateOnly.FromDateTime(VersionDateFrom.Value)
                : DateOnly.FromDateTime(DateTime.Today);

            EditModel.Version.DateTo = VersionDateTo.HasValue
                ? DateOnly.FromDateTime(VersionDateTo.Value)
                : null;

            EditModel.Version.Ingredients = Ingredients.ToList();
        }

        /// <summary>Перезаполняет справочники блюд, единиц и продуктов.</summary>
        private async Task LoadOptionsAsync()
        {
            var dishes = await _dishService.GetAllAsync();
            var units = await _unitService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            DishOptions.Clear();
            foreach (var dish in dishes)
            {
                DishOptions.Add(dish);
            }

            UnitOptions.Clear();
            foreach (var unit in units)
            {
                UnitOptions.Add(unit);
            }

            ProductOptions.Clear();
            foreach (var product in products)
            {
                ProductOptions.Add(product);
            }
        }

        private void ReplaceItems(IEnumerable<RecipeCardListItemDto> items)
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
            (CreateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddIngredientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveIngredientCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
