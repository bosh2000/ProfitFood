using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;

namespace ProfitFood.UI.ViewModels.Reference.AgeGroups
{
    /// <summary>
    /// Справочник возрастных групп детей.
    /// </summary>
    public sealed class AgeGroupsReferenceViewModel : ReferenceCrudViewModelBase<AgeGroupDto, AgeGroupDto>
    {
        private readonly IAgeGroupAppService _service;

        /// <summary>
        /// Инициализирует справочник возрастных групп.
        /// </summary>
        /// <param name="service">Сервис справочника.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервис не передан.</exception>
        public AgeGroupsReferenceViewModel(IAgeGroupAppService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Title = "Возрастные группы";
        }

        /// <summary>Загружает все возрастные группы.</summary>
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

        /// <summary>Ищет возрастные группы по введённому тексту.</summary>
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

        protected override AgeGroupDto CreateNewEditModel() => new() { AgeFromMonths = 12, AgeToMonths = 36 };

        protected override AgeGroupDto BuildEditModel(AgeGroupDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            AgeFromMonths = item.AgeFromMonths,
            AgeToMonths = item.AgeToMonths
        };

        protected override Task SaveCoreAsync(AgeGroupDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(AgeGroupDto item) => _service.DeleteAsync(item);
    }
}
