using AutoMapper;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    public class UnitRepository : RepositoryBase<Unit>, IUnitRepository
    {
        public UnitRepository(ProfitFoodDbContext context, IMapper mapper) : base(context)
        {
        }
    }
}