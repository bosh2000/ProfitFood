using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    internal class UnitRepository : RepositoryBase<Unit>, IUnitRepository
    {
        public UnitRepository(ProfitFoodDbContext context) : base(context)
        {
        }
    }
}