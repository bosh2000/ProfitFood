using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Model.DBModel;
using ProfitFoot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.DAL.Repository.Implementation.ItemRepository
{
    internal class BaseUnitRepository : RepositoryBase<BaseUnit>, IBaseUnitRepository
    {
        public BaseUnitRepository(ProfitFoodDbContext context) : base(context)
        {
        }
    }
}