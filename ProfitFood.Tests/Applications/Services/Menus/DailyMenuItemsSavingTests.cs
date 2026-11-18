using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Dto.Menus;
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
    /// Проверяет сохранение блюд меню: добавление, обновление и удаление позиций.
    /// </summary>
    public sealed class DailyMenuItemsSavingTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProfitFoodDbContext> _options;
        private readonly DailyMenuAppService _service;
        private readonly Guid _dailyMenuId;
        private readonly Guid _breakfastId;
        private readonly Guid _dinnerId;
        private readonly Guid _porridgeVersionId;
        private readonly Guid _soupVersionId;
        private readonly Guid _porridgeDishId;
        private readonly Guid _soupDishId;

        public DailyMenuItemsSavingTests()
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

                var breakfast = new MealType { Name = "Завтрак", Code = "B", SortOrder = 1 };
                var dinner = new MealType { Name = "Обед", Code = "D", SortOrder = 2 };
                var category = new DishCategory { Name = "Блюда", SortOrder = 1 };

                context.Units.Add(gram);
                context.MealTypes.AddRange(breakfast, dinner);
                context.DishCategories.Add(category);
                context.SaveChanges();

                var porridge = new Dish
                {
                    Name = "Каша молочная",
                    Code = "Д-001",
                    DishCategoryId = category.Id,
                    MealTypeId = breakfast.Id
                };

                var soup = new Dish
                {
                    Name = "Суп овощной",
                    Code = "Д-002",
                    DishCategoryId = category.Id,
                    MealTypeId = dinner.Id
                };

                context.Dishes.AddRange(porridge, soup);
                context.SaveChanges();

                var porridgeCard = new RecipeCard
                {
                    DishId = porridge.Id,
                    CardNumber = "ТК-001",
                    Name = "Каша молочная"
                };

                var soupCard = new RecipeCard
                {
                    DishId = soup.Id,
                    CardNumber = "ТК-002",
                    Name = "Суп овощной"
                };

                context.RecipeCards.AddRange(porridgeCard, soupCard);
                context.SaveChanges();

                var porridgeVersion = new RecipeCardVersion
                {
                    RecipeCardId = porridgeCard.Id,
                    VersionNumber = 1,
                    DateFrom = new DateOnly(2026, 1, 1),
                    OutputQuantity = 200m,
                    OutputUnitId = gram.Id,
                    IsApproved = true
                };

                var soupVersion = new RecipeCardVersion
                {
                    RecipeCardId = soupCard.Id,
                    VersionNumber = 1,
                    DateFrom = new DateOnly(2026, 1, 1),
                    OutputQuantity = 250m,
                    OutputUnitId = gram.Id,
                    IsApproved = true
                };

                context.RecipeCardVersions.AddRange(porridgeVersion, soupVersion);

                var menu = new DailyMenu
                {
                    MenuDate = DateOnly.FromDateTime(DateTime.Today),
                    Status = DailyMenuStatus.Draft,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.DailyMenus.Add(menu);
                context.SaveChanges();

                _dailyMenuId = menu.Id;
                _breakfastId = breakfast.Id;
                _dinnerId = dinner.Id;
                _porridgeVersionId = porridgeVersion.Id;
                _soupVersionId = soupVersion.Id;
                _porridgeDishId = porridge.Id;
                _soupDishId = soup.Id;
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
        public async Task SaveItemsAsync_ShouldStorePositionsInOrder()
        {
            await _service.SaveItemsAsync(_dailyMenuId, new[]
            {
                CreateRow(_breakfastId, _porridgeVersionId),
                CreateRow(_dinnerId, _soupVersionId)
            });

            var items = await _service.GetItemsAsync(_dailyMenuId);

            items.Should().HaveCount(2);
            items.Select(x => x.MealTypeName).Should().ContainInOrder("Завтрак", "Обед");
            items.Select(x => x.DishName).Should().ContainInOrder("Каша молочная", "Суп овощной");
        }

        [Fact]
        public async Task SaveItemsAsync_ShouldRemovePositionsThatAreMissingInIncomingList()
        {
            await _service.SaveItemsAsync(_dailyMenuId, new[]
            {
                CreateRow(_breakfastId, _porridgeVersionId),
                CreateRow(_dinnerId, _soupVersionId)
            });

            var stored = await _service.GetItemsAsync(_dailyMenuId);
            var soupRow = stored.Single(x => x.MealTypeId == _dinnerId);

            // Оставляем только суп: строка каши должна исчезнуть.
            await _service.SaveItemsAsync(_dailyMenuId, new[] { soupRow });

            var items = await _service.GetItemsAsync(_dailyMenuId);

            items.Should().ContainSingle();
            items.Single().DishName.Should().Be("Суп овощной");
        }

        [Fact]
        public async Task SaveItemsAsync_WhenMenuIdIsEmpty_ShouldThrow()
        {
            var act = async () => await _service.SaveItemsAsync(Guid.Empty, Array.Empty<DailyMenuItemDto>());

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveItemsAsync_WhenDishHasNoCard_ShouldThrow()
        {
            var row = CreateRow(_breakfastId, Guid.Empty);

            var act = async () => await _service.SaveItemsAsync(_dailyMenuId, new[] { row });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private DailyMenuItemDto CreateRow(Guid mealTypeId, Guid versionId) => new()
        {
            MealTypeId = mealTypeId,
            DishId = versionId == _porridgeVersionId ? _porridgeDishId : _soupDishId,
            RecipeCardVersionId = versionId
        };

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
