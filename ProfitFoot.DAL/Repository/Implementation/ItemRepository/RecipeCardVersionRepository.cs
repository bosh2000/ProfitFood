using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation.ItemRepository
{
    /// <summary>Репозиторий версий технологических карт.</summary>
    public class RecipeCardVersionRepository : RepositoryBase<RecipeCardVersion>, IRecipeCardVersionRepository
    {
        /// <summary>Инициализирует репозиторий.</summary>
        /// <param name="contextFactory">Фабрика, выдающая контекст на каждую операцию.</param>
        public RecipeCardVersionRepository(IDbContextFactory<ProfitFoodDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
}
