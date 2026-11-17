namespace ProfitFood.Applications.Services.Interfaces
{
    public interface ISeasonAppService
    {
        Task<IReadOnlyCollection<Dto.References.SeasonDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Dto.References.SeasonDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(Dto.References.SeasonDto model, CancellationToken cancellationToken = default);

        Task DeleteAsync(Dto.References.SeasonDto model, CancellationToken cancellationToken = default);
    }
}
