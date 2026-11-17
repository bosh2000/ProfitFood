using ProfitFood.Applications.Dto.Menus;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IDailyMenuAppService
    {
        Task<IReadOnlyCollection<DailyMenuListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<DailyMenuListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(DailyMenuListItemDto model, CancellationToken cancellationToken = default);

        /// <summary>Возвращает позиции меню: приём пищи, блюдо, техкарту и выход порции.</summary>
        Task<IReadOnlyCollection<DailyMenuItemDto>> GetItemsAsync(Guid dailyMenuId, CancellationToken cancellationToken = default);

        Task DeleteAsync(DailyMenuListItemDto model, CancellationToken cancellationToken = default);
    }
}
