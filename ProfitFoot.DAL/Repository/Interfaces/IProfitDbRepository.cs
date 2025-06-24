using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Implementation.ItemRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.DAL.Repository.Interfaces
{
    public interface IProfitDbRepository
    {
        IBaseUnitRepository BaseUnitRepository { get; }
        IBaseUnitStorageRepository BaseUnitStorageRepository { get; }
        IProductGroupRepository ProductGroupRepository { get; }
        IProductRepository ProductRepository { get; }
    }
}