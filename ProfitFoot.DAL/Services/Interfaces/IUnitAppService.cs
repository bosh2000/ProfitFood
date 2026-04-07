using ProfitFood.Domain.ModelsViewModels;

namespace ProfitFood.Infrastructure.Services.Interfaces
{
    public interface IUnitAppService
    {
        Task<IReadOnlyCollection<UnitListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<UnitListItemViewModel>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(UnitEditModel model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}