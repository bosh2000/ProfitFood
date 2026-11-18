using ProfitFood.Applications.Dto.Menus;
using ProfitFood.Applications.Dto.Recipes;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Список ежедневных меню с возможностью создать и изменить запись.
    /// </summary>
    public sealed class DailyMenuListViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IDailyMenuAppService _service;
        private readonly ISeasonAppService _seasonService;
        private readonly IDailyMenuAttendanceAppService _attendanceService;
        private readonly IMealTypeAppService _mealTypeService;
        private readonly IDishAppService _dishService;
        private readonly IRecipeCardAppService _recipeCardService;

        private DailyMenuListItemDto? _selectedItem;
        private DailyMenuListItemDto _editModel = new();
        private DateTime? _editDate = DateTime.Today;
        private string _searchText = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;
        private bool _isEditMode;
        private bool _isCreateMode;
        private DailyMenuItemDto? _selectedPosition;
        private Guid? _newPositionMealTypeId;
        private Guid? _newPositionDishId;
        private Guid? _newPositionRecipeCardId;

        /// <summary>
        /// Инициализирует список меню.
        /// </summary>
        /// <param name="service">Сервис ежедневных меню.</param>
        /// <param name="seasonService">Сервис справочника сезонов для выпадающего списка.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public DailyMenuListViewModel(
            IDailyMenuAppService service,
            ISeasonAppService seasonService,
            IDailyMenuAttendanceAppService attendanceService,
            IMealTypeAppService mealTypeService,
            IDishAppService dishService,
            IRecipeCardAppService recipeCardService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _seasonService = seasonService ?? throw new ArgumentNullException(nameof(seasonService));
            _attendanceService = attendanceService ?? throw new ArgumentNullException(nameof(attendanceService));
            _mealTypeService = mealTypeService ?? throw new ArgumentNullException(nameof(mealTypeService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _recipeCardService = recipeCardService ?? throw new ArgumentNullException(nameof(recipeCardService));

            Items = new ObservableCollection<DailyMenuListItemDto>();
            SeasonOptions = new ObservableCollection<SeasonDto>();
            AttendanceRows = new ObservableCollection<DailyMenuAttendanceDto>();
            Positions = new ObservableCollection<DailyMenuItemDto>();
            MealTypeOptions = new ObservableCollection<MealTypeDto>();
            DishOptions = new ObservableCollection<DishDto>();
            RecipeCardOptions = new ObservableCollection<RecipeCardListItemDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedItem is not null && !IsBusy);
            CreateCommand = new RelayCommand(_ => StartCreate(), _ => !IsBusy);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => Cancel(), _ => !IsBusy);
            SaveAttendanceCommand = new RelayCommand(
                async _ => await SaveAttendanceAsync(),
                _ => !IsBusy && EditModel.Id != Guid.Empty);
            AddPositionCommand = new RelayCommand(async _ => await AddPositionAsync(), _ => !IsBusy);
            RemovePositionCommand = new RelayCommand(_ => RemovePosition(), _ => !IsBusy && SelectedPosition is not null);
        }

        /// <summary>Меню, начиная с самого свежего.</summary>
        public ObservableCollection<DailyMenuListItemDto> Items { get; }

        /// <summary>Сезоны, доступные для выбора.</summary>
        public ObservableCollection<SeasonDto> SeasonOptions { get; }

        /// <summary>Посещаемость по группам детей для выбранного меню.</summary>
        public ObservableCollection<DailyMenuAttendanceDto> AttendanceRows { get; }

        /// <summary>Блюда выбранного меню.</summary>
        public ObservableCollection<DailyMenuItemDto> Positions { get; }

        /// <summary>Приёмы пищи для добавления блюда.</summary>
        public ObservableCollection<MealTypeDto> MealTypeOptions { get; }

        /// <summary>Блюда для добавления в меню.</summary>
        public ObservableCollection<DishDto> DishOptions { get; }

        /// <summary>Техкарты выбранного блюда.</summary>
        public ObservableCollection<RecipeCardListItemDto> RecipeCardOptions { get; }

        /// <summary>Заголовок раздела.</summary>
        public string Title => "Меню";

        /// <summary>Выбранное меню.</summary>
        public DailyMenuListItemDto? SelectedItem
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
                    _ = LoadAttendanceAsync(value.Id);
                    _ = LoadPositionsAsync(value.Id);
                }

                UpdateCommandState();
            }
        }

        /// <summary>Модель, привязанная к карточке редактирования.</summary>
        public DailyMenuListItemDto EditModel
        {
            get => _editModel;
            private set => SetProperty(ref _editModel, value);
        }

        /// <summary>
        /// Дата меню для DatePicker: DateOnly нельзя привязать к элементу напрямую,
        /// поэтому в форме живёт DateTime.
        /// </summary>
        public DateTime? EditDate
        {
            get => _editDate;
            set => SetProperty(ref _editDate, value);
        }

        /// <summary>Текст поиска по примечанию.</summary>
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

        /// <summary>Выбранное блюдо в таблице меню.</summary>
        public DailyMenuItemDto? SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                if (SetProperty(ref _selectedPosition, value))
                {
                    (RemovePositionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>Приём пищи для добавляемого блюда.</summary>
        public Guid? NewPositionMealTypeId
        {
            get => _newPositionMealTypeId;
            set => SetProperty(ref _newPositionMealTypeId, value);
        }

        /// <summary>Блюдо для добавления; смена блюда перезаполняет список техкарт.</summary>
        public Guid? NewPositionDishId
        {
            get => _newPositionDishId;
            set
            {
                if (SetProperty(ref _newPositionDishId, value))
                {
                    NewPositionRecipeCardId = null;
                    _ = LoadRecipeCardsAsync(value);
                }
            }
        }

        /// <summary>Техкарта для добавляемого блюда.</summary>
        public Guid? NewPositionRecipeCardId
        {
            get => _newPositionRecipeCardId;
            set => SetProperty(ref _newPositionRecipeCardId, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveAttendanceCommand { get; }
        public ICommand AddPositionCommand { get; }
        public ICommand RemovePositionCommand { get; }

        /// <summary>Загружает список при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Загружает все меню и справочник сезонов.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadSeasonOptionsAsync();
                await LoadOptionsAsync();

                ReplaceItems(await _service.GetAllAsync());
                StatusMessage = $"Загружено меню: {Items.Count}.";
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

        /// <summary>Сохраняет запись и перезагружает список.</summary>
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                EditModel.MenuDate = EditDate.HasValue
                    ? DateOnly.FromDateTime(EditDate.Value)
                    : DateOnly.FromDateTime(DateTime.Today);

                await _service.SaveAsync(EditModel);
                await LoadAsync();

                // Карточка не сбрасывается: у нового меню появился Id, и только теперь
                // можно сохранить блюда и посещаемость.
                var saved = Items.FirstOrDefault(x => x.MenuDate == EditModel.MenuDate);
                if (saved is not null)
                {
                    await _service.SaveItemsAsync(saved.Id, Positions.ToList());
                    SelectedItem = saved;
                }

                StatusMessage = "Меню сохранено.";
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

        /// <summary>Удаляет выбранное меню.</summary>
        private async Task DeleteAsync()
        {
            if (SelectedItem is null)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _service.DeleteAsync(SelectedItem);
                SelectedItem = null;
                await LoadAsync();
                StatusMessage = "Меню удалено.";
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

        /// <summary>Ищет меню по введённому тексту.</summary>
        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;
                ReplaceItems(await _service.SearchAsync(SearchText));
                StatusMessage = $"Найдено меню: {Items.Count}.";
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
            EditModel = new DailyMenuListItemDto();
            EditDate = DateTime.Today;
            Positions.Clear();
            AttendanceRows.Clear();
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
        /// Перезаполняет список сезонов для выпадающего списка карточки.
        /// </summary>
        private async Task LoadSeasonOptionsAsync()
        {
            var seasons = await _seasonService.GetAllAsync();

            SeasonOptions.Clear();
            foreach (var season in seasons)
            {
                SeasonOptions.Add(season);
            }
        }

        private void ReplaceItems(IEnumerable<DailyMenuListItemDto> items)
        {
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        /// <summary>Загружает посещаемость по группам детей для выбранного меню.</summary>
        private async Task LoadAttendanceAsync(Guid dailyMenuId)
        {
            try
            {
                AttendanceRows.Clear();

                foreach (var row in await _attendanceService.GetByMenuAsync(dailyMenuId))
                {
                    AttendanceRows.Add(row);
                }

                if (AttendanceRows.Count == 0)
                {
                    StatusMessage = "В справочнике «Группы детей» нет записей — посещаемость заполнять нечем.";
                }
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки посещаемости: {exception.Message}";
            }
        }

        /// <summary>Сохраняет посещаемость по группам детей.</summary>
        private async Task SaveAttendanceAsync()
        {
            try
            {
                IsBusy = true;

                await _attendanceService.SaveAsync(EditModel.Id, AttendanceRows.ToList());
                StatusMessage = "Посещаемость сохранена.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка сохранения посещаемости: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Загружает справочники приёмов пищи и блюд для добавления в меню.</summary>
        private async Task LoadOptionsAsync()
        {
            var mealTypes = await _mealTypeService.GetAllAsync();
            var dishes = await _dishService.GetAllAsync();

            MealTypeOptions.Clear();
            foreach (var mealType in mealTypes)
            {
                MealTypeOptions.Add(mealType);
            }

            DishOptions.Clear();
            foreach (var dish in dishes)
            {
                DishOptions.Add(dish);
            }
        }

        /// <summary>Загружает техкарты выбранного блюда: какую именно готовить, выбирает пользователь.</summary>
        private async Task LoadRecipeCardsAsync(Guid? dishId)
        {
            RecipeCardOptions.Clear();

            if (dishId is null || dishId == Guid.Empty)
            {
                return;
            }

            var cards = await _recipeCardService.GetAllAsync();

            foreach (var card in cards.Where(x => x.DishId == dishId.Value))
            {
                RecipeCardOptions.Add(card);
            }
        }

        /// <summary>Загружает блюда выбранного меню.</summary>
        private async Task LoadPositionsAsync(Guid dailyMenuId)
        {
            try
            {
                Positions.Clear();

                foreach (var position in await _service.GetItemsAsync(dailyMenuId))
                {
                    Positions.Add(position);
                }
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки блюд меню: {exception.Message}";
            }
        }

        /// <summary>Добавляет блюдо в список меню по значениям панели добавления.</summary>
        private async Task AddPositionAsync()
        {
            if (NewPositionMealTypeId is null || NewPositionMealTypeId == Guid.Empty)
            {
                StatusMessage = "Выберите приём пищи для блюда.";
                return;
            }

            if (NewPositionDishId is null || NewPositionDishId == Guid.Empty)
            {
                StatusMessage = "Выберите блюдо.";
                return;
            }

            if (NewPositionRecipeCardId is null || NewPositionRecipeCardId == Guid.Empty)
            {
                StatusMessage = "Выберите техкарту блюда.";
                return;
            }

            try
            {
                var card = await _recipeCardService.GetByIdAsync(NewPositionRecipeCardId.Value);

                if (card is null || card.Version.Id == Guid.Empty)
                {
                    StatusMessage = "У выбранной техкарты нет версии: заполните её в разделе «Техкарты».";
                    return;
                }

                if (Positions.Any(x => x.RecipeCardVersionId == card.Version.Id))
                {
                    StatusMessage = "Это блюдо уже добавлено в меню.";
                    return;
                }

                var mealType = MealTypeOptions.FirstOrDefault(x => x.Id == NewPositionMealTypeId.Value);
                var dish = DishOptions.FirstOrDefault(x => x.Id == NewPositionDishId.Value);

                Positions.Add(new DailyMenuItemDto
                {
                    MealTypeId = NewPositionMealTypeId.Value,
                    MealTypeName = mealType?.Name ?? string.Empty,
                    DishId = NewPositionDishId.Value,
                    DishName = dish?.Name ?? string.Empty,
                    RecipeCardVersionId = card.Version.Id,
                    RecipeCardNumber = card.CardNumber,
                    OutputQuantity = card.Version.OutputQuantity,
                    OutputUnitName = card.Version.OutputUnitName,
                    SortOrder = Positions.Count + 1
                });

                StatusMessage = "Блюдо добавлено в меню.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка добавления блюда: {exception.Message}";
            }
        }

        /// <summary>Убирает выбранное блюдо из списка меню.</summary>
        private void RemovePosition()
        {
            if (SelectedPosition is null)
            {
                return;
            }

            Positions.Remove(SelectedPosition);
            SelectedPosition = null;
            StatusMessage = "Блюдо убрано из меню.";
        }

        private static DailyMenuListItemDto BuildEditModel(DailyMenuListItemDto item) => new()
        {
            Id = item.Id,
            MenuDate = item.MenuDate,
            SeasonId = item.SeasonId,
            SeasonName = item.SeasonName,
            StatusName = item.StatusName,
            Notes = item.Notes,
            UpdatedAt = item.UpdatedAt
        };

        private void UpdateCommandState()
        {
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CreateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveAttendanceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddPositionCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemovePositionCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
