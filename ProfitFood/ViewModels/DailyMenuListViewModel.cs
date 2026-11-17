using ProfitFood.Applications.Dto.Menus;
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

        private DailyMenuListItemDto? _selectedItem;
        private DailyMenuListItemDto _editModel = new();
        private DateTime? _editDate = DateTime.Today;
        private string _searchText = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;
        private bool _isEditMode;
        private bool _isCreateMode;

        /// <summary>
        /// Инициализирует список меню.
        /// </summary>
        /// <param name="service">Сервис ежедневных меню.</param>
        /// <param name="seasonService">Сервис справочника сезонов для выпадающего списка.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public DailyMenuListViewModel(IDailyMenuAppService service, ISeasonAppService seasonService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _seasonService = seasonService ?? throw new ArgumentNullException(nameof(seasonService));

            Items = new ObservableCollection<DailyMenuListItemDto>();
            SeasonOptions = new ObservableCollection<SeasonDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedItem is not null && !IsBusy);
            CreateCommand = new RelayCommand(_ => StartCreate(), _ => !IsBusy);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => Cancel(), _ => !IsBusy);
        }

        /// <summary>Меню, начиная с самого свежего.</summary>
        public ObservableCollection<DailyMenuListItemDto> Items { get; }

        /// <summary>Сезоны, доступные для выбора.</summary>
        public ObservableCollection<SeasonDto> SeasonOptions { get; }

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

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Загружает список при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Загружает все меню и справочник сезонов.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadSeasonOptionsAsync();

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

                StartCreate();
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
        }
    }
}
