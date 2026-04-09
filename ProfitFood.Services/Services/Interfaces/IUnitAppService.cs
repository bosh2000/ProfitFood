using ProfitFood.Applications.Dto.References;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IUnitAppService
    {
        Task<IReadOnlyCollection<UnitItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<UnitItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(UnitEditDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}