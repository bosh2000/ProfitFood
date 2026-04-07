using Microsoft.Extensions.DependencyInjection;
using ProfitFood.UI.ViewModels.Reference.Units;
using ProfitFood.UI.Views.Reference;
using System.Collections.ObjectModel;

namespace ProfitFood.UI.ViewModels.Reference
{
    public sealed class ReferencesViewModel : ViewModelBase
    {
        private ViewModelBase? _currentReferenceViewModel;
        private ReferenceSectionItem? _selectedSection;

        private IServiceProvider _serviceProvider;

        public IServiceProvider ServiceProvider
        {
            set { this._serviceProvider = value; }
        }

        public ObservableCollection<ReferenceSectionItem> Sections { get; } = new();

        public ReferenceSectionItem? SelectedSection
        {
            get => _selectedSection;
            set
            {
                if (SetProperty(ref _selectedSection, value) && value != null)
                {
                    OpenSection(value);
                }
            }
        }

        public ViewModelBase? CurrentReferenceViewModel
        {
            get => _currentReferenceViewModel;
            set => SetProperty(ref _currentReferenceViewModel, value);
        }

        public ReferencesViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Sections.Add(new ReferenceSectionItem("Единицы измерения", "Units"));
            Sections.Add(new ReferenceSectionItem("Категории продуктов", "ProductCategories"));
            Sections.Add(new ReferenceSectionItem("Продукты", "Products"));
            Sections.Add(new ReferenceSectionItem("Возрастные группы", "AgeGroups"));
            Sections.Add(new ReferenceSectionItem("Группы детей", "ChildGroups"));
            Sections.Add(new ReferenceSectionItem("Типы приемов пищи", "MealTypes"));
            Sections.Add(new ReferenceSectionItem("Категории блюд", "DishCategories"));
            Sections.Add(new ReferenceSectionItem("Сезоны", "Seasons"));
            Sections.Add(new ReferenceSectionItem("Склады", "StorageLocations"));

            SelectedSection = Sections.FirstOrDefault();
        }

        private void OpenSection(ReferenceSectionItem section)
        {
            CurrentReferenceViewModel = section.Key switch
            {
                "Units" => _serviceProvider.GetRequiredService<UnitsReferenceViewModel>(),
                "ProductCategories" => new ProductCategoriesReferenceViewModel(),
                "Products" => new ProductsReferenceViewModel(),
                "AgeGroups" => new AgeGroupsReferenceViewModel(),
                "ChildGroups" => new ChildGroupsReferenceViewModel(),
                "MealTypes" => new MealTypesReferenceViewModel(),
                "DishCategories" => new DishCategoriesReferenceViewModel(),
                "Seasons" => new SeasonsReferenceViewModel(),
                "StorageLocations" => new StorageLocationsReferenceViewModel(),
                _ => null
            };
        }
    }
}