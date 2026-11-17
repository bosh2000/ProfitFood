using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;
using System.Collections.ObjectModel;

namespace ProfitFood.UI.ViewModels.Reference.ChildGroups
{
    /// <summary>
    /// Справочник групп детей в ДОУ.
    /// </summary>
    public sealed class ChildGroupsReferenceViewModel : ReferenceCrudViewModelBase<ChildGroupDto, ChildGroupDto>
    {
        private readonly IChildGroupAppService _service;
        private readonly IAgeGroupAppService _ageGroupService;

        /// <summary>
        /// Инициализирует справочник групп детей.
        /// </summary>
        /// <param name="service">Сервис справочника групп детей.</param>
        /// <param name="ageGroupService">Сервис справочника возрастных групп для выпадающего списка.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public ChildGroupsReferenceViewModel(IChildGroupAppService service, IAgeGroupAppService ageGroupService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _ageGroupService = ageGroupService ?? throw new ArgumentNullException(nameof(ageGroupService));

            Title = "Группы детей";
            AgeGroupOptions = new ObservableCollection<AgeGroupDto>();
        }

        /// <summary>
        /// Возрастные группы, доступные для выбора в карточке записи.
        /// </summary>
        public ObservableCollection<AgeGroupDto> AgeGroupOptions { get; }

        /// <summary>Загружает группы детей и список возрастных групп.</summary>
        public override async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadAgeGroupOptionsAsync();
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

        /// <summary>Ищет группы детей по введённому тексту.</summary>
        protected override async Task SearchAsync()
        {
            try
            {
                IsBusy = true;

                await LoadAgeGroupOptionsAsync();
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

        protected override ChildGroupDto CreateNewEditModel() => new();

        protected override ChildGroupDto BuildEditModel(ChildGroupDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            AgeGroupId = item.AgeGroupId,
            AgeGroupName = item.AgeGroupName,
            PlannedChildrenCount = item.PlannedChildrenCount
        };

        protected override Task SaveCoreAsync(ChildGroupDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(ChildGroupDto item) => _service.DeleteAsync(item);

        /// <summary>
        /// Перезаполняет список возрастных групп для выпадающего списка карточки.
        /// </summary>
        private async Task LoadAgeGroupOptionsAsync()
        {
            var ageGroups = await _ageGroupService.GetAllAsync();

            AgeGroupOptions.Clear();
            foreach (var ageGroup in ageGroups)
            {
                AgeGroupOptions.Add(ageGroup);
            }
        }
    }
}
