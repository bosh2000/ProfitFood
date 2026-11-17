using Microsoft.Extensions.DependencyInjection;
using ProfitFood.UI.ViewModels.DailyMenuEditorViewModels;
using ProfitFood.UI.ViewModels.DashBoardViewModels;
using ProfitFood.UI.ViewModels.Navigation;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Каркас гибридной навигации: слева список разделов, справа — вкладки открытых экранов.
    /// Раздел открывается единственной вкладкой, документ — новой вкладкой при каждом создании.
    /// </summary>
    public sealed class MainWindowViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        private NavigationSectionItem? _selectedSection;
        private TabItemViewModel? _selectedTab;
        private string _statusText = "Готово";

        /// <summary>
        /// Инициализирует каркас навигации и открывает первый раздел.
        /// </summary>
        /// <param name="serviceProvider">Контейнер, из которого разрешаются ViewModel разделов и документов.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="serviceProvider"/> равно null.</exception>
        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            Sections = new ObservableCollection<NavigationSectionItem>
            {
                new("Главная", typeof(DashboardViewModel)),
                new("Меню", typeof(DailyMenuListViewModel)),
                new("Техкарты", typeof(RecipeCardsViewModel)),
                new("Склад", typeof(StockViewModel)),
                new("Документы", typeof(DocumentsViewModel)),
                new("Справочники", typeof(ReferencesViewModel))
            };

            Tabs = new ObservableCollection<TabItemViewModel>();

            CreateMenuCommand = new RelayCommand(_ => CreateMenuDocument());
            CloseTabCommand = new RelayCommand(_ => CloseTab(SelectedTab), _ => SelectedTab is not null);

            // Первый раздел выбирается только после того, как контейнер и коллекция вкладок готовы.
            SelectedSection = Sections[0];
        }

        /// <summary>
        /// Разделы приложения, отображаемые в боковом меню.
        /// </summary>
        public ObservableCollection<NavigationSectionItem> Sections { get; }

        /// <summary>
        /// Открытые вкладки: разделы и документы.
        /// </summary>
        public ObservableCollection<TabItemViewModel> Tabs { get; }

        /// <summary>
        /// Выбранный раздел меню. Выбор сразу открывает или активирует его вкладку.
        /// </summary>
        public NavigationSectionItem? SelectedSection
        {
            get => _selectedSection;
            set
            {
                if (SetProperty(ref _selectedSection, value) && value is not null)
                {
                    OpenSection(value);
                }
            }
        }

        /// <summary>
        /// Текущая активная вкладка.
        /// </summary>
        public TabItemViewModel? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        /// <summary>
        /// Текст строки состояния.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Команда создания нового документа «Меню».
        /// </summary>
        public ICommand CreateMenuCommand { get; }

        /// <summary>
        /// Команда закрытия активной вкладки.
        /// </summary>
        public ICommand CloseTabCommand { get; }

        /// <summary>
        /// Открывает раздел: повторный выбор активирует уже открытую вкладку.
        /// </summary>
        /// <param name="section">Выбранный раздел.</param>
        private void OpenSection(NavigationSectionItem section)
        {
            var existingTab = Tabs.FirstOrDefault(tab => Equals(tab.Key, section.ViewModelType));

            if (existingTab is null)
            {
                existingTab = CreateTab(section.ViewModelType, section.Title, section.ViewModelType);
                Tabs.Add(existingTab);
            }

            SelectedTab = existingTab;
            StatusText = $"Открыт раздел «{section.Title}»";
        }

        /// <summary>
        /// Создаёт документ «Меню» в новой вкладке.
        /// </summary>
        private void CreateMenuDocument()
        {
            var header = $"Меню от {DateTime.Today:dd.MM.yyyy}";
            var tab = CreateTab(typeof(DailyMenuEditorViewModel), header, Guid.NewGuid());

            Tabs.Add(tab);
            SelectedTab = tab;
            StatusText = "Создан новый документ «Меню»";
        }

        /// <summary>
        /// Разрешает ViewModel через контейнер и оборачивает её во вкладку.
        /// </summary>
        /// <param name="viewModelType">Тип ViewModel.</param>
        /// <param name="header">Заголовок вкладки.</param>
        /// <param name="key">Ключ вкладки.</param>
        /// <returns>Готовая вкладка.</returns>
        private TabItemViewModel CreateTab(Type viewModelType, string header, object key)
        {
            var viewModel = _serviceProvider.GetRequiredService(viewModelType);
            var tab = new TabItemViewModel(key, header, viewModel, CloseTab);

            // Загрузка данных не должна задерживать открытие вкладки.
            _ = InitializeAsync(viewModel);

            return tab;
        }

        /// <summary>
        /// Выполняет отложенную инициализацию ViewModel, если она её поддерживает.
        /// </summary>
        /// <param name="viewModel">ViewModel открываемой вкладки.</param>
        private async Task InitializeAsync(object viewModel)
        {
            if (viewModel is not IInitializableViewModel initializable)
            {
                return;
            }

            try
            {
                await initializable.InitializeAsync();
            }
            catch (Exception exception)
            {
                // Ошибку показываем в строке состояния; с полным стеком она уже уйдёт в журнал.
                StatusText = $"Ошибка загрузки раздела: {exception.Message}";
            }
        }

        /// <summary>
        /// Закрывает вкладку и освобождает ресурсы её ViewModel.
        /// </summary>
        /// <param name="tab">Закрываемая вкладка.</param>
        private void CloseTab(TabItemViewModel? tab)
        {
            if (tab is null)
            {
                return;
            }

            var index = Tabs.IndexOf(tab);
            if (index < 0)
            {
                return;
            }

            Tabs.RemoveAt(index);

            if (ReferenceEquals(SelectedTab, tab))
            {
                SelectedTab = Tabs.Count == 0 ? null : Tabs[Math.Min(index, Tabs.Count - 1)];
            }

            // Долгоживущие ViewModel могут держать подписки — освобождаем при закрытии вкладки.
            if (tab.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }

            StatusText = $"Вкладка «{tab.Header}» закрыта";
        }
    }
}
