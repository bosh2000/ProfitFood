using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Model.DBModel;
using ProfitFoot.DAL;

namespace ProfitFood.DAL.Repository.Implementation.ItemRepository
{
    internal class BaseUnitRepository : RepositoryBase<BaseUnit>, IBaseUnitRepository
    {
        public BaseUnitRepository(ProfitFoodDbContext context) : base(context)
        {
        }
    }
}