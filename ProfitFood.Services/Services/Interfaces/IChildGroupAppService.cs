namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IChildGroupAppService
    {
        Task<IReadOnlyCollection<Dto.References.ChildGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.ChildGroupDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.ChildGroupDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.ChildGroupDto model, CancellationToken cancellationToken = default);
    }
}
