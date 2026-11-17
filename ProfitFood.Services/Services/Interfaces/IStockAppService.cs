using ProfitFood.Applications.Dto.Stocks;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IStockAppService
    {
        Task<IReadOnlyCollection<StockBalanceDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<StockBalanceDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        Task SaveAsync(StockBalanceDto model, CancellationToken cancellationToken = default);
    }
}
