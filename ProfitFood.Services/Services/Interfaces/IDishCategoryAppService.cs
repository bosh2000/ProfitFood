namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IDishCategoryAppService
    {
        Task<IReadOnlyCollection<Dto.References.DishCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.DishCategoryDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.DishCategoryDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.DishCategoryDto model, CancellationToken cancellationToken = default);
    }
}
