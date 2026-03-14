using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Implementation;
using ProfitFoot.Infrastructure;

namespace ProfitFood.DAL.Repository.Implementation.ItemRepository
{
    internal class ProductGroupRepositiry : RepositoryBase<ProductCategory>, IProductCategoryRepository
    {
        public ProductGroupRepositiry(ProfitFoodDbContext context) : base(context)
        {
        }
    }
}