using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.Documents;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    /// <summary>Репозиторий остатков продуктов на складах.</summary>
    public class StockBalanceRepository : RepositoryBase<StockBalance>, IStockBalanceRepository
    {
        /// <summary>Инициализирует репозиторий.</summary>
        /// <param name="contextFactory">Фабрика, выдающая контекст на каждую операцию.</param>
        public StockBalanceRepository(IDbContextFactory<ProfitFoodDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
}
