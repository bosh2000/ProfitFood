using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;

namespace ProfitFood.UI.ViewModels.Reference.MealTypes
{
    /// <summary>
    /// Справочник типов приёмов пищи.
    /// </summary>
    public sealed class MealTypesReferenceViewModel : ReferenceCrudViewModelBase<MealTypeDto, MealTypeDto>
    {
        private readonly IMealTypeAppService _service;

        /// <summary>
        /// Инициализирует справочник типов приёмов пищи.
        /// </summary>
        /// <param name="service">Сервис справочника.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервис не передан.</exception>
        public MealTypesReferenceViewModel(IMealTypeAppService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Title = "Типы приёмов пищи";
        }

        /// <summary>Загружает все типы приёмов пищи.</summary>
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

        /// <summary>Ищет типы приёмов пищи по введённому тексту.</summary>
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

        protected override MealTypeDto CreateNewEditModel() => new() { SortOrder = 1 };

        protected override MealTypeDto BuildEditModel(MealTypeDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code,
            SortOrder = item.SortOrder
        };

        protected override Task SaveCoreAsync(MealTypeDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(MealTypeDto item) => _service.DeleteAsync(item);
    }
}
