using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    /// <summary>Репозиторий справочника категорий продуктов.</summary>
    public class ProductCategoryRepository : RepositoryBase<ProductCategory>, IProductCategoryRepository
    {
        /// <summary>Инициализирует репозиторий.</summary>
        /// <param name="contextFactory">Фабрика, выдающая контекст на каждую операцию.</param>
        /// <param name="mapper">Маппер, сохранённый для будущих проекций.</param>
        public ProductCategoryRepository(IDbContextFactory<ProfitFoodDbContext> contextFactory, IMapper mapper) : base(contextFactory)
        {
        }
    }
}
