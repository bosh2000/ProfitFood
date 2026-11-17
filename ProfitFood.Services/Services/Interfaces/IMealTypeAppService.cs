namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IMealTypeAppService
    {
        Task<IReadOnlyCollection<Dto.References.MealTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.MealTypeDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.MealTypeDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.MealTypeDto model, CancellationToken cancellationToken = default);
    }
}
