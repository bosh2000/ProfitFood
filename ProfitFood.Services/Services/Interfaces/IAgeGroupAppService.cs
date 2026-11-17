namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IAgeGroupAppService
    {
        Task<IReadOnlyCollection<Dto.References.AgeGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.AgeGroupDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.AgeGroupDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.AgeGroupDto model, CancellationToken cancellationToken = default);
    }
}
