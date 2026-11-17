using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Menus;
using ProfitFoot.Infrastructure;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Menus
{
    /// <summary>
    /// Проверяет чтение позиций меню на реальной базе SQLite в памяти:
    /// позиции собираются из нескольких таблиц, моками это не проверить.
    /// </summary>
    public sealed class DailyMenuItemsTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProfitFoodDbContext> _options;
        private readonly DailyMenuAppService _service;
        private readonly Guid _dailyMenuId;

        public DailyMenuItemsTests()
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
                var gram = new Unit
                {
                    Name = "Грамм",
                    ShortName = "г",
                    UnitType = UnitType.Weigth,
                    BaseFactor = 0.001m,
                    IsBase = true
                };

                var mealType = new MealType { Name = "Завтрак", Code = "B", SortOrder = 1 };
                var category = new DishCategory { Name = "Каши", SortOrder = 1 };

                context.Units.Add(gram);
                context.MealTypes.Add(mealType);
                context.DishCategories.Add(category);
                context.SaveChanges();

                var dish = new Dish
                {
                    Name = "Каша молочная",
                    Code = "Д-001",
                    DishCategoryId = category.Id,
                    MealTypeId = mealType.Id
                };

                context.Dishes.Add(dish);
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

                var menu = new DailyMenu
                {
                    MenuDate = DateOnly.FromDateTime(DateTime.Today),
                    Status = ProfitFood.Domain.Entities.Enums.DailyMenuStatus.Draft,
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

            _service = new DailyMenuAppService(
                new Mock<IDbRepository>().Object,
                new TestContextFactory(_options),
                NullLogger<DailyMenuAppService>.Instance);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task GetItemsAsync_ShouldFillMealTypeDishCardAndOutput()
        {
            var result = await _service.GetItemsAsync(_dailyMenuId);

            var item = result.Should().ContainSingle().Subject;
            item.MealTypeName.Should().Be("Завтрак");
            item.DishName.Should().Be("Каша молочная");
            item.RecipeCardNumber.Should().Be("ТК-054");
            item.OutputQuantity.Should().Be(200m);
            item.OutputUnitName.Should().Be("г");
        }

        [Fact]
        public async Task GetItemsAsync_WhenMenuHasNoItems_ShouldReturnEmpty()
        {
            Guid emptyMenuId;

            using (var context = new ProfitFoodDbContext(_options))
            {
                var menu = new DailyMenu
                {
                    MenuDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
                    Status = ProfitFood.Domain.Entities.Enums.DailyMenuStatus.Draft,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.DailyMenus.Add(menu);
                context.SaveChanges();

                emptyMenuId = menu.Id;
            }

            var result = await _service.GetItemsAsync(emptyMenuId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetItemsAsync_WhenIdIsEmpty_ShouldReturnEmpty()
        {
            var result = await _service.GetItemsAsync(Guid.Empty);

            result.Should().BeEmpty();
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
