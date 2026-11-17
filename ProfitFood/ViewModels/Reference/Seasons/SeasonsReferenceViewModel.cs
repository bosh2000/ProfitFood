using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;

namespace ProfitFood.UI.ViewModels.Reference.Seasons
{
    /// <summary>
    /// Справочник сезонов меню.
    /// </summary>
    public sealed class SeasonsReferenceViewModel : ReferenceCrudViewModelBase<SeasonDto, SeasonDto>
    {
        private readonly ISeasonAppService _service;

        /// <summary>
        /// Инициализирует справочник сезонов.
        /// </summary>
        /// <param name="service">Сервис справочника.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервис не передан.</exception>
        public SeasonsReferenceViewModel(ISeasonAppService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Title = "Сезоны";
        }

        /// <summary>Загружает все сезоны.</summary>
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

        /// <summary>Ищет сезоны по введённому тексту.</summary>
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

        protected override SeasonDto CreateNewEditModel() => new() { MonthFrom = 1, MonthTo = 12 };

        protected override SeasonDto BuildEditModel(SeasonDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            MonthFrom = item.MonthFrom,
            MonthTo = item.MonthTo
        };

        protected override Task SaveCoreAsync(SeasonDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(SeasonDto item) => _service.DeleteAsync(item);
    }
}
