using ProfitFood.Applications.Dto.Menus;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IDailyMenuAppService
    {
        Task<IReadOnlyCollection<DailyMenuListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<DailyMenuListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(DailyMenuListItemDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(DailyMenuListItemDto model, CancellationToken cancellationToken = default);
    }
}
