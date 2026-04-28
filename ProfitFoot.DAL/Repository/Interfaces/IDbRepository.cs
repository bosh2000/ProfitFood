using ProfitFood.DAL.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Infrastructure.Repository.Interfaces
{
    public interface IDbRepository
    {
        IUnitRepository unitRepository { get; }
        IProductCategoryRepository productCategoryRepository { get; }
    }
}