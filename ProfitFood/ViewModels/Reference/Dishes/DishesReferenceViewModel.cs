using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;
using System.Collections.ObjectModel;

namespace ProfitFood.UI.ViewModels.Reference.Dishes
{
    /// <summary>
    /// Справочник блюд.
    /// </summary>
    public sealed class DishesReferenceViewModel : ReferenceCrudViewModelBase<DishDto, DishDto>
    {
        private readonly IDishAppService _service;
        private readonly IDishCategoryAppService _categoryService;
        private readonly IMealTypeAppService _mealTypeService;

        /// <summary>
        /// Инициализирует справочник блюд.
        /// </summary>
        /// <param name="service">Сервис блюд.</param>
        /// <param name="categoryService">Сервис категорий блюд для выпадающего списка.</param>
        /// <param name="mealTypeService">Сервис приёмов пищи для выпадающего списка.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public DishesReferenceViewModel(
            IDishAppService service,
            IDishCategoryAppService categoryService,
            IMealTypeAppService mealTypeService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _mealTypeService = mealTypeService ?? throw new ArgumentNullException(nameof(mealTypeService));

            DishCategoryOptions = new ObservableCollection<DishCategoryDto>();
            MealTypeOptions = new ObservableCollection<MealTypeDto>();

            Title = "Блюда";
        }

        /// <summary>Категории блюд для выбора в карточке.</summary>
        public ObservableCollection<DishCategoryDto> DishCategoryOptions { get; }

        /// <summary>Приёмы пищи для выбора в карточке.</summary>
        public ObservableCollection<MealTypeDto> MealTypeOptions { get; }

        /// <inheritdoc />
        public override async Task LoadAsync()
        {
            await LoadOptionsAsync();
            ReplaceItems(await _service.GetAllAsync());
        }

        /// <inheritdoc />
        protected override async Task SearchAsync()
            => ReplaceItems(await _service.SearchAsync(SearchText));

        /// <inheritdoc />
        protected override DishDto CreateNewEditModel() => new();

        /// <inheritdoc />
        protected override DishDto BuildEditModel(DishDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code,
            DishCategoryId = item.DishCategoryId,
            DishCategoryName = item.DishCategoryName,
            MealTypeId = item.MealTypeId,
            MealTypeName = item.MealTypeName,
            Notes = item.Notes
        };

        /// <inheritdoc />
        protected override Task SaveCoreAsync(DishDto model) => _service.SaveAsync(model);

        /// <inheritdoc />
        protected override Task DeleteCoreAsync(DishDto item) => _service.DeleteAsync(item);

        /// <summary>
        /// Перезаполняет справочники категорий и приёмов пищи для карточки.
        /// </summary>
        private async Task LoadOptionsAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            var mealTypes = await _mealTypeService.GetAllAsync();

            DishCategoryOptions.Clear();
            foreach (var category in categories)
            {
                DishCategoryOptions.Add(category);
            }

            MealTypeOptions.Clear();
            foreach (var mealType in mealTypes)
            {
                MealTypeOptions.Add(mealType);
            }
        }
    }
}
