namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IStorageLocationAppService
    {
        Task<IReadOnlyCollection<Dto.References.StorageLocationDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.StorageLocationDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.StorageLocationDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.StorageLocationDto model, CancellationToken cancellationToken = default);
    }
}
