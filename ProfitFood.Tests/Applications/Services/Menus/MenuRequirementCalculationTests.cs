using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Menus;
using ProfitFoot.Infrastructure;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Menus
{
    /// <summary>
    /// Проверяет расчёт меню-требования на реальной базе SQLite в памяти:
    /// расчёт сводит меню, техкарты, ингредиенты и посещаемость, моками это не проверить.
    /// </summary>
    public sealed class MenuRequirementCalculationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProfitFoodDbContext> _options;
        private readonly MenuRequirementAppService _service;
        private readonly Guid _dailyMenuId;
        private readonly Guid _emptyMenuId;

        public MenuRequirementCalculationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ProfitFoodDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new ProfitFoodDbContext(_options))
            {
                context.Database.EnsureCreated();
            }

            using (var context = new ProfitFoodDbContext(_options))
            {
                var kilogram = new Unit
                {
                    Name = "Килограмм",
                    ShortName = "кг",
                    UnitType = UnitType.Weigth,
                    BaseFactor = 1m,
                    IsBase = true
                };

                var gram = new Unit
                {
                    Name = "Грамм",
                    ShortName = "г",
                    UnitType = UnitType.Weigth,
                    BaseFactor = 0.001m,
                    IsBase = false
                };

                var category = new ProductCategory { Name = "Молочные продукты" };
                var dishCategory = new DishCategory { Name = "Каши", SortOrder = 1 };
                var mealType = new MealType { Name = "Завтрак", Code = "B", SortOrder = 1 };
                var location = new StorageLocation { Name = "Основной склад", Code = "01" };
                var ageGroup = new AgeGroup { Name = "Младшая", AgeFromMonths = 12, AgeToMonths = 36 };

                context.Units.AddRange(kilogram, gram);
                context.ProductCategories.Add(category);
                context.DishCategories.Add(dishCategory);
                context.MealTypes.Add(mealType);
                context.StorageLocations.Add(location);
                context.AgeGroups.Add(ageGroup);
                context.SaveChanges();

                var product = new Product
                {
                    Name = "Молоко",
                    FullName = "Молоко 3,2%",
                    ProductCategoryId = category.Id,
                    BaseUnitId = kilogram.Id,
                    MinStock = 1m
                };

                var dish = new Dish
                {
                    Name = "Каша молочная",
                    Code = "Д-001",
                    DishCategoryId = dishCategory.Id,
                    MealTypeId = mealType.Id
                };

                context.Products.Add(product);
                context.Dishes.Add(dish);
                context.SaveChanges();

                var childGroup = new ChildGroup
                {
                    Name = "Группа 1",
                    AgeGroupId = ageGroup.Id,
                    PlannedChildrenCount = 10
                };

                context.ChildGroups.Add(childGroup);
                context.SaveChanges();

                var card = new RecipeCard
                {
                    DishId = dish.Id,
                    CardNumber = "ТК-054",
                    Name = "Каша молочная"
                };

                context.RecipeCards.Add(card);
                context.SaveChanges();

                var version = new RecipeCardVersion
                {
                    RecipeCardId = card.Id,
                    VersionNumber = 1,
                    DateFrom = new DateOnly(2026, 1, 1),
                    OutputQuantity = 200m,
                    OutputUnitId = gram.Id,
                    IsApproved = true
                };

                context.RecipeCardVersions.Add(version);
                context.SaveChanges();

                // Норма на одну порцию: 0,2 кг молока.
                context.RecipeIngredients.Add(new RecipeIngredient
                {
                    RecipeCardVersionId = version.Id,
                    ProductId = product.Id,
                    GrossQuantity = 0.2m,
                    NetQuantity = 0.2m,
                    UnitId = kilogram.Id,
                    LossPercent = 0m,
                    SortOrder = 1
                });

                var menu = new DailyMenu
                {
                    MenuDate = DateOnly.FromDateTime(DateTime.Today),
                    Status = DailyMenuStatus.Draft,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var emptyMenu = new DailyMenu
                {
                    MenuDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
                    Status = DailyMenuStatus.Draft,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.DailyMenus.AddRange(menu, emptyMenu);
                context.SaveChanges();

                context.DailyMenuItems.Add(new DailyMenuItem
                {
                    DailyMenuId = menu.Id,
                    MealTypeId = mealType.Id,
                    DishId = dish.Id,
                    RecipeCardVersionId = version.Id,
                    SortOrder = 1
                });

                context.SaveChanges();

                _dailyMenuId = menu.Id;
                _emptyMenuId = emptyMenu.Id;
            }

            _service = new MenuRequirementAppService(
                new Mock<IDbRepository>().Object,
                new TestContextFactory(_options),
                NullLogger<MenuRequirementAppService>.Instance);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task CalculateAsync_ShouldMultiplyIngredientByChildrenCount()
        {
            var id = await _service.CalculateAsync(_dailyMenuId);

            var document = await _service.GetByIdAsync(id);

            document.Should().NotBeNull();
            document!.TotalChildrenCount.Should().Be(10);
            document.Number.Should().StartWith("МТ-");
            document.Status.Should().Be(MenuRequirementStatus.Draft);

            var line = document.Items.Should().ContainSingle().Subject;
            line.ProductName.Should().Be("Молоко");
            // 10 детей по 0,2 кг дают 2 кг в базовой единице продукта.
            line.Quantity.Should().Be(2m);
        }

        [Fact]
        public async Task CalculateAsync_WhenMenuHasNoPositions_ShouldThrow()
        {
            var act = async () => await _service.CalculateAsync(_emptyMenuId);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CalculateAsync_WhenMenuIdIsEmpty_ShouldThrow()
        {
            var act = async () => await _service.CalculateAsync(Guid.Empty);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        /// <summary>
        /// Фабрика контекста поверх одного открытого соединения: иначе каждый новый контекст
        /// создавал бы собственную пустую базу в памяти.
        /// </summary>
        private sealed class TestContextFactory : IDbContextFactory<ProfitFoodDbContext>
        {
            private readonly DbContextOptions<ProfitFoodDbContext> _options;

            public TestContextFactory(DbContextOptions<ProfitFoodDbContext> options)
            {
                _options = options;
            }

            public ProfitFoodDbContext CreateDbContext() => new(_options);
        }
    }
}
