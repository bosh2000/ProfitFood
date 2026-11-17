using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProfitFood.DAL.Repository.Implementation.ItemRepository;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Infrastructure.Repository.Implementation.ItemRepository;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Repository.Implementation
{
    /// <summary>
    /// Объединяет конкретные репозитории и передаёт им фабрику контекста вместо одного общего контекста.
    /// </summary>
    public class DbRepository : IDbRepository
    {
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует агрегирующий репозиторий.
        /// </summary>
        /// <param name="contextFactory">Фабрика, выдающая контекст на каждую операцию.</param>
        /// <param name="mapper">Маппер, передаваемый конкретным репозиториям.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="contextFactory"/> равно null.</exception>
        public DbRepository(IDbContextFactory<ProfitFoodDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mapper = mapper;
        }

        /// <inheritdoc />
        public IUnitRepository unitRepository => new UnitRepository(_contextFactory, _mapper);

        /// <inheritdoc />
        public IProductCategoryRepository productCategoryRepository => new ProductCategoryRepository(_contextFactory, _mapper);

        /// <inheritdoc />
        public IAgeGroupRepository ageGroupRepository => new AgeGroupRepository(_contextFactory);

        /// <inheritdoc />
        public IChildGroupRepository childGroupRepository => new ChildGroupRepository(_contextFactory);

        /// <inheritdoc />
        public IDishCategoryRepository dishCategoryRepository => new DishCategoryRepository(_contextFactory);

        /// <inheritdoc />
        public IMealTypeRepository mealTypeRepository => new MealTypeRepository(_contextFactory);

        /// <inheritdoc />
        public IProductRepository productRepository => new ProductRepository(_contextFactory);

        /// <inheritdoc />
        public ISeasonRepository seasonRepository => new SeasonRepository(_contextFactory);

        /// <inheritdoc />
        public IStorageLocationRepository storageLocationRepository => new StorageLocationRepository(_contextFactory);

        /// <inheritdoc />
        public IDishRepository dishRepository => new DishRepository(_contextFactory);

        /// <inheritdoc />
        public IDailyMenuRepository dailyMenuRepository => new DailyMenuRepository(_contextFactory);

        /// <inheritdoc />
        public IRecipeCardRepository recipeCardRepository => new RecipeCardRepository(_contextFactory);

        /// <inheritdoc />
        public IRecipeCardVersionRepository recipeCardVersionRepository => new RecipeCardVersionRepository(_contextFactory);

        /// <inheritdoc />
        public IMenuRequirementRepository menuRequirementRepository => new MenuRequirementRepository(_contextFactory);

        /// <inheritdoc />
        public IStockBalanceRepository stockBalanceRepository => new StockBalanceRepository(_contextFactory);

        /// <inheritdoc />
        public IStockDocumentRepository stockDocumentRepository => new StockDocumentRepository(_contextFactory);
    }
}
