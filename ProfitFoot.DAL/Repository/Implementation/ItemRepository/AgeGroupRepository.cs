using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    /// <summary>Репозиторий справочника возрастных групп.</summary>
    public class AgeGroupRepository : RepositoryBase<AgeGroup>, IAgeGroupRepository
    {
        /// <summary>Инициализирует репозиторий.</summary>
        /// <param name="contextFactory">Фабрика, выдающая контекст на каждую операцию.</param>
        public AgeGroupRepository(IDbContextFactory<ProfitFoodDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
}
