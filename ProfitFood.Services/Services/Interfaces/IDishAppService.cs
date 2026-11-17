using ProfitFood.Applications.Dto.References;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IDishAppService
    {
        Task<IReadOnlyCollection<DishDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<DishDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(DishDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(DishDto model, CancellationToken cancellationToken = default);
    }
}
