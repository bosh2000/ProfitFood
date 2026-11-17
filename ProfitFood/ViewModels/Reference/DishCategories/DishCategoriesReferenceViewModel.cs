using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;

namespace ProfitFood.UI.ViewModels.Reference.DishCategories
{
    /// <summary>
    /// Справочник категорий блюд.
    /// </summary>
    public sealed class DishCategoriesReferenceViewModel : ReferenceCrudViewModelBase<DishCategoryDto, DishCategoryDto>
    {
        private readonly IDishCategoryAppService _service;

        /// <summary>
        /// Инициализирует справочник категорий блюд.
        /// </summary>
        /// <param name="service">Сервис справочника.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервис не передан.</exception>
        public DishCategoriesReferenceViewModel(IDishCategoryAppService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Title = "Категории блюд";
        }

        /// <summary>Загружает все категории блюд.</summary>
        public override async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                ReplaceItems(await _service.GetAllAsync());
                StatusMessage = $"Загружено записей: {Items.Count}.";
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

        /// <summary>Ищет категории блюд по введённому тексту.</summary>
        protected override async Task SearchAsync()
        {
            try
            {
                IsBusy = true;
                ReplaceItems(await _service.SearchAsync(SearchText));
                StatusMessage = $"Найдено записей: {Items.Count}.";
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

        protected override DishCategoryDto CreateNewEditModel() => new();

        protected override DishCategoryDto BuildEditModel(DishCategoryDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            SortOrder = item.SortOrder
        };

        protected override Task SaveCoreAsync(DishCategoryDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(DishCategoryDto item) => _service.DeleteAsync(item);
    }
}
