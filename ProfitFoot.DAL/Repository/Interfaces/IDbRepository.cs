using ProfitFood.DAL.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Repository.Interfaces
{
    public interface IDbRepository
    {
        IUnitRepository unitRepository { get; }
        IProductCategoryRepository productCategoryRepository { get; }

        /// <summary>Возрастные группы детей.</summary>
        IAgeGroupRepository ageGroupRepository { get; }

        /// <summary>Группы детей в ДОУ.</summary>
        IChildGroupRepository childGroupRepository { get; }

        /// <summary>Категории блюд.</summary>
        IDishCategoryRepository dishCategoryRepository { get; }

        /// <summary>Блюда.</summary>
        IDishRepository dishRepository { get; }

        /// <summary>Типы приёмов пищи.</summary>
        IMealTypeRepository mealTypeRepository { get; }

        /// <summary>Продукты питания.</summary>
        IProductRepository productRepository { get; }

        /// <summary>Сезоны меню.</summary>
        ISeasonRepository seasonRepository { get; }

        /// <summary>Места хранения продуктов.</summary>
        IStorageLocationRepository storageLocationRepository { get; }

        /// <summary>Ежедневные меню.</summary>
        IDailyMenuRepository dailyMenuRepository { get; }

        /// <summary>Технологические карты.</summary>
        IRecipeCardRepository recipeCardRepository { get; }

        /// <summary>Версии технологических карт.</summary>
        IRecipeCardVersionRepository recipeCardVersionRepository { get; }

        /// <summary>Остатки продуктов на складах.</summary>
        IStockBalanceRepository stockBalanceRepository { get; }

        /// <summary>Складские документы.</summary>
        IStockDocumentRepository stockDocumentRepository { get; }
    }
}
