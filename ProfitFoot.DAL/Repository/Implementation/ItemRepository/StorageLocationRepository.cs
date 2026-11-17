using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    /// <summary>Репозиторий справочника складов.</summary>
    public class StorageLocationRepository : RepositoryBase<StorageLocation>, IStorageLocationRepository
    {
        /// <summary>Инициализирует репозиторий.</summary>
        /// <param name="contextFactory">Фабрика, выдающая контекст на каждую операцию.</param>
        public StorageLocationRepository(IDbContextFactory<ProfitFoodDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
}
