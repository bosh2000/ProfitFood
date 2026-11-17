using Microsoft.Extensions.DependencyInjection;
using ProfitFood.UI.ViewModels.Reference.AgeGroups;
using ProfitFood.UI.ViewModels.Reference.ChildGroups;
using ProfitFood.UI.ViewModels.Reference.DishCategories;
using ProfitFood.UI.ViewModels.Reference.Dishes;
using ProfitFood.UI.ViewModels.Reference.MealTypes;
using ProfitFood.UI.ViewModels.Reference.ProductCategories;
using ProfitFood.UI.ViewModels.Reference.Products;
using ProfitFood.UI.ViewModels.Reference.Seasons;
using ProfitFood.UI.ViewModels.Reference.StorageLocations;
using ProfitFood.UI.ViewModels.Reference.Units;
using System.Collections.ObjectModel;

namespace ProfitFood.UI.ViewModels.Reference
{
    /// <summary>
    /// Раздел справочников: список нормативных данных слева и выбранный справочник справа.
    /// </summary>
    public sealed class ReferencesViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        private ReferenceSectionItem? _selectedSection;
        private ViewModelBase? _currentReferenceViewModel;
        private string _statusMessage = "Выберите справочник.";
        private bool _isInitialized;

        /// <summary>
        /// Инициализирует раздел справочников.
        /// </summary>
        /// <param name="serviceProvider">Контейнер, из которого разрешаются ViewModel справочников.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="serviceProvider"/> равно null.</exception>
        public ReferencesViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Список типизирован: ключом служит сам тип ViewModel, поэтому switch по строкам не нужен.
            Sections = new ObservableCollection<ReferenceSectionItem>
            {
                new("Единицы измерения", typeof(UnitsReferenceViewModel)),
                new("Категории продуктов", typeof(ProductCategoriesViewModel)),
                new("Продукты", typeof(ProductsReferenceViewModel)),
                new("Возрастные группы", typeof(AgeGroupsReferenceViewModel)),
                new("Группы детей", typeof(ChildGroupsReferenceViewModel)),
                new("Типы приемов пищи", typeof(MealTypesReferenceViewModel)),
                new("Категории блюд", typeof(DishCategoriesReferenceViewModel)),
                new("Блюда", typeof(DishesReferenceViewModel)),
                new("Сезоны", typeof(SeasonsReferenceViewModel)),
                new("Склады", typeof(StorageLocationsReferenceViewModel))
            };
        }

        /// <summary>
        /// Список справочников.
        /// </summary>
        public ObservableCollection<ReferenceSectionItem> Sections { get; }

        /// <summary>
        /// Выбранный справочник. Выбор сразу открывает его справа.
        /// </summary>
        public ReferenceSectionItem? SelectedSection
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
        /// ViewModel открытого справочника.
        /// </summary>
        public ViewModelBase? CurrentReferenceViewModel
        {
            get => _currentReferenceViewModel;
            private set => SetProperty(ref _currentReferenceViewModel, value);
        }

        /// <summary>
        /// Сообщение о состоянии загрузки справочника.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Открывает первый справочник при первом показе раздела.
        /// </summary>
        /// <returns>Задача завершается после запуска загрузки выбранного справочника.</returns>
        public Task InitializeAsync()
        {
            if (_isInitialized)
            {
                return Task.CompletedTask;
            }

            _isInitialized = true;

            // Навигация запускается не из конструктора: к этому моменту раздел уже привязан к интерфейсу.
            SelectedSection = Sections.FirstOrDefault();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Открывает справочник по выбранному пункту.
        /// </summary>
        /// <param name="section">Выбранный пункт списка справочников.</param>
        private async void OpenSection(ReferenceSectionItem section)
        {
            try
            {
                var viewModel = (ViewModelBase)_serviceProvider.GetRequiredService(section.ViewModelType);
                CurrentReferenceViewModel = viewModel;

                if (viewModel is IInitializableViewModel initializable)
                {
                    await initializable.InitializeAsync();
                }

                StatusMessage = $"Открыт справочник «{section.Title}».";
            }
            catch (Exception exception)
            {
                // Пункт меню остаётся выбранным, но пользователь видит причину вместо пустой панели.
                CurrentReferenceViewModel = null;
                StatusMessage = $"Не удалось открыть справочник: {exception.Message}";
            }
        }
    }
}
