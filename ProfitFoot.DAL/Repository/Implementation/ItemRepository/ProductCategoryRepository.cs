using AutoMapper;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFoot.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    public class ProductCategoryRepository : RepositoryBase<ProductCategory>, IProductCategoryRepository
    {
        public ProductCategoryRepository(ProfitFoodDbContext context, IMapper mapper) : base(context)
        {
        }
    }
}