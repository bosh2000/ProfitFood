namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IProductAppService
    {
        Task<IReadOnlyCollection<Dto.References.ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.ProductDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.ProductDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.ProductDto model, CancellationToken cancellationToken = default);
    }
}
