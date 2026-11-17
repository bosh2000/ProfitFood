using ProfitFood.Applications.Dto.Menus;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Журнал меню-требований с расчётом потребности по меню на дату.
    /// </summary>
    public sealed class MenuRequirementsViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IMenuRequirementAppService _service;
        private readonly IDailyMenuAppService _menuService;

        private MenuRequirementDto? _selectedItem;
        private MenuRequirementDto _editModel = new();
        private DailyMenuListItemDto? _selectedMenuOption;
        private string _statusMessage = "Готово";
        private bool _isBusy;

        /// <summary>
        /// Инициализирует раздел меню-требований.
        /// </summary>
        /// <param name="service">Сервис меню-требований.</param>
        /// <param name="menuService">Сервис меню: источник для расчёта.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public MenuRequirementsViewModel(IMenuRequirementAppService service, IDailyMenuAppService menuService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));

            Items = new ObservableCollection<MenuRequirementDto>();
            DocumentItems = new ObservableCollection<MenuRequirementItemDto>();
            MenuOptions = new ObservableCollection<DailyMenuListItemDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            CalculateCommand = new RelayCommand(
                async _ => await CalculateAsync(),
                _ => !IsBusy && SelectedMenuOption is not null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy && EditModel.Id != Guid.Empty);
            DeleteCommand = new RelayCommand(
                async _ => await DeleteAsync(),
                _ => !IsBusy && EditModel.Id != Guid.Empty);
        }

        /// <summary>Журнал требований, начиная с самого свежего.</summary>
        public ObservableCollection<MenuRequirementDto> Items { get; }

        /// <summary>Строки открытого требования.</summary>
        public ObservableCollection<MenuRequirementItemDto> DocumentItems { get; }

        /// <summary>Меню, доступные для расчёта.</summary>
        public ObservableCollection<DailyMenuListItemDto> MenuOptions { get; }

        /// <summary>Заголовок раздела.</summary>
        public string Title => "Требования";

        /// <summary>Выбранное в журнале требование.</summary>
        public MenuRequirementDto? SelectedItem
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

        /// <summary>Открытое требование в карточке.</summary>
        public MenuRequirementDto EditModel
        {
            get => _editModel;
            private set => SetProperty(ref _editModel, value);
        }

        /// <summary>Меню, выбранное для расчёта.</summary>
        public DailyMenuListItemDto? SelectedMenuOption
        {
            get => _selectedMenuOption;
            set
            {
                if (SetProperty(ref _selectedMenuOption, value))
                {
                    (CalculateCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
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
        public ICommand CalculateCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        /// <summary>Загружает журнал при первом показе раздела.</summary>
        public Task InitializeAsync() => LoadAsync();

        /// <summary>Загружает журнал требований и список меню для расчёта.</summary>
        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadMenuOptionsAsync();

                ReplaceItems(await _service.GetAllAsync());
                StatusMessage = $"Загружено требований: {Items.Count}.";
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

        /// <summary>Рассчитывает требование по выбранному меню.</summary>
        private async Task CalculateAsync()
        {
            if (SelectedMenuOption is null)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var id = await _service.CalculateAsync(SelectedMenuOption.Id);
                await LoadAsync();

                var document = await _service.GetByIdAsync(id);
                if (document is not null)
                {
                    ApplyDocument(document);
                }

                StatusMessage = $"Требование рассчитано по меню на {SelectedMenuOption.MenuDate:dd.MM.yyyy}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка расчёта: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Сохраняет правки черновика.</summary>
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                EditModel.Items = DocumentItems.ToList();

                await _service.SaveAsync(EditModel);
                await LoadAsync();

                StatusMessage = "Требование сохранено.";
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

        /// <summary>Удаляет черновик требования.</summary>
        private async Task DeleteAsync()
        {
            try
            {
                IsBusy = true;

                await _service.DeleteAsync(EditModel);
                ApplyDocument(new MenuRequirementDto());

                await LoadAsync();
                StatusMessage = "Требование удалено.";
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

        /// <summary>Загружает требование со строками.</summary>
        private async Task LoadDocumentAsync(Guid id)
        {
            try
            {
                var document = await _service.GetByIdAsync(id);

                if (document is null)
                {
                    StatusMessage = "Требование не найдено: возможно, его удалили.";
                    return;
                }

                ApplyDocument(document);
                StatusMessage = $"Открыто требование {document.Number}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки требования: {exception.Message}";
            }
        }

        /// <summary>Перезаполняет список меню, доступных для расчёта.</summary>
        private async Task LoadMenuOptionsAsync()
        {
            var menus = await _menuService.GetAllAsync();

            MenuOptions.Clear();
            foreach (var menu in menus)
            {
                MenuOptions.Add(menu);
            }
        }

        private void ApplyDocument(MenuRequirementDto document)
        {
            EditModel = document;

            DocumentItems.Clear();
            foreach (var item in document.Items)
            {
                DocumentItems.Add(item);
            }

            if (document.Id == Guid.Empty)
            {
                SelectedItem = null;
            }
        }

        private void ReplaceItems(IEnumerable<MenuRequirementDto> items)
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
            (CalculateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
