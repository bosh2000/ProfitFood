using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.ViewModels.Reference.Base;

namespace ProfitFood.UI.ViewModels.Reference.StorageLocations
{
    /// <summary>
    /// Справочник мест хранения продуктов.
    /// </summary>
    public sealed class StorageLocationsReferenceViewModel : ReferenceCrudViewModelBase<StorageLocationDto, StorageLocationDto>
    {
        private readonly IStorageLocationAppService _service;

        /// <summary>
        /// Инициализирует справочник складов.
        /// </summary>
        /// <param name="service">Сервис справочника.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервис не передан.</exception>
        public StorageLocationsReferenceViewModel(IStorageLocationAppService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Title = "Склады";
        }

        /// <summary>Загружает все места хранения.</summary>
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

        /// <summary>Ищет места хранения по введённому тексту.</summary>
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

        protected override StorageLocationDto CreateNewEditModel() => new();

        protected override StorageLocationDto BuildEditModel(StorageLocationDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code
        };

        protected override Task SaveCoreAsync(StorageLocationDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(StorageLocationDto item) => _service.DeleteAsync(item);
    }
}
