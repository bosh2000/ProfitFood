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
    internal class BaseUnitStorageRepository : RepositoryBase<BaseUnitStorage>, IBaseUnitStorageRepository
    {
        public BaseUnitStorageRepository(ProfitFoodDbContext context) : base(context)
        {
        }
    }
}