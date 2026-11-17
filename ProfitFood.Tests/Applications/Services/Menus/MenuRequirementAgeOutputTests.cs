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
    /// Проверяет, что расчёт меню-требования учитывает выход блюда по возрастным группам.
    /// </summary>
    public sealed class MenuRequirementAgeOutputTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProfitFoodDbContext> _options;
        private readonly MenuRequirementAppService _service;
        private readonly Guid _dailyMenuId;

        public MenuRequirementAgeOutputTests()
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

                var nursery = new AgeGroup { Name = "Младшая", AgeFromMonths = 12, AgeToMonths = 36 };
                var senior = new AgeGroup { Name = "Старшая", AgeFromMonths = 36, AgeToMonths = 84 };
                var category = new ProductCategory { Name = "Молочные продукты" };
                var dishCategory = new DishCategory { Name = "Каши", SortOrder = 1 };
                var mealType = new MealType { Name = "Завтрак", Code = "B", SortOrder = 1 };
                var location = new StorageLocation { Name = "Основной склад", Code = "01" };

                context.Units.AddRange(kilogram, gram);
                context.AgeGroups.AddRange(nursery, senior);
                context.ProductCategories.Add(category);
                context.DishCategories.Add(dishCategory);
                context.MealTypes.Add(mealType);
                context.StorageLocations.Add(location);
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

                // Две группы детей разного возраста: 10 младших и 20 старших.
                var nurseryGroup = new ChildGroup
                {
                    Name = "Группа 1",
                    AgeGroupId = nursery.Id,
                    PlannedChildrenCount = 10
                };

                var seniorGroup = new ChildGroup
                {
                    Name = "Группа 2",
                    AgeGroupId = senior.Id,
                    PlannedChildrenCount = 20
                };

                context.Products.Add(product);
                context.Dishes.Add(dish);
                context.ChildGroups.AddRange(nurseryGroup, seniorGroup);
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

                // Норма на одну стандартную порцию: 0,2 кг молока.
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

                // Младшим — 100 г, старшим — 150 г при стандартном выходе 200 г.
                context.DishOutputs.AddRange(
                    new DishOutput
                    {
                        RecipeCardVersionId = version.Id,
                        AgeGroupId = nursery.Id,
                        OutputQuantity = 100m,
                        UnitId = gram.Id
                    },
                    new DishOutput
                    {
                        RecipeCardVersionId = version.Id,
                        AgeGroupId = senior.Id,
                        OutputQuantity = 150m,
                        UnitId = gram.Id
                    });

                var menu = new DailyMenu
                {
                    MenuDate = DateOnly.FromDateTime(DateTime.Today),
                    Status = DailyMenuStatus.Draft,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.DailyMenus.Add(menu);
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
        public async Task CalculateAsync_ShouldApplyOutputCoefficientPerAgeGroup()
        {
            var id = await _service.CalculateAsync(_dailyMenuId);

            var document = await _service.GetByIdAsync(id);

            document.Should().NotBeNull();
            document!.TotalChildrenCount.Should().Be(30);

            // 10 детей × 0,2 кг × (100/200) + 20 детей × 0,2 кг × (150/200) = 1 + 3 = 4 кг.
            var line = document.Items.Should().ContainSingle().Subject;
            line.Quantity.Should().Be(4m);
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
