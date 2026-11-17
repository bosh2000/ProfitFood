using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Dto.Recipes;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Recipes;
using ProfitFoot.Infrastructure;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Recipes
{
    /// <summary>
    /// Проверяет сохранение техкарты с версией и ингредиентами на реальной базе SQLite в памяти:
    /// связанные сущности нельзя проверить моками, они живут в SQL.
    /// </summary>
    public sealed class RecipeCardSavingTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProfitFoodDbContext> _options;
        private readonly RecipeCardAppService _service;
        private readonly Guid _dishId;
        private readonly Guid _unitId;
        private readonly Guid _productId;

        public RecipeCardSavingTests()
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
                // Идентификаторы генерируются в конструкторах сущностей, поэтому читаются после сохранения.
                var dishCategory = new DishCategory { Name = "Супы", SortOrder = 1 };
                var mealType = new MealType { Name = "Обед", Code = "L", SortOrder = 2 };
                var unit = new Unit
                {
                    Name = "Грамм",
                    ShortName = "г",
                    UnitType = UnitType.Weigth,
                    BaseFactor = 1m,
                    IsBase = true
                };
                var productCategory = new ProductCategory { Name = "Овощи" };

                context.DishCategories.Add(dishCategory);
                context.MealTypes.Add(mealType);
                context.Units.Add(unit);
                context.ProductCategories.Add(productCategory);
                context.SaveChanges();

                var dish = new Dish
                {
                    Name = "Борщ",
                    Code = "С-001",
                    DishCategoryId = dishCategory.Id,
                    MealTypeId = mealType.Id
                };

                var product = new Product
                {
                    Name = "Свёкла",
                    FullName = "Свёкла свежая",
                    ProductCategoryId = productCategory.Id,
                    BaseUnitId = unit.Id,
                    MinStock = 0m
                };

                context.Dishes.Add(dish);
                context.Products.Add(product);
                context.SaveChanges();

                _dishId = dish.Id;
                _unitId = unit.Id;
                _productId = product.Id;
            }

            var repositoryMock = new Mock<IDbRepository>();
            _service = new RecipeCardAppService(
                repositoryMock.Object,
                new TestContextFactory(_options),
                NullLogger<RecipeCardAppService>.Instance);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task SaveAsync_WhenCardIsNew_ShouldCreateCardWithVersionAndIngredient()
        {
            var model = BuildModel("ТК-001", "Борщ со сметаной");

            var id = await _service.SaveAsync(model);

            id.Should().NotBe(Guid.Empty);

            var saved = await _service.GetByIdAsync(id);

            saved.Should().NotBeNull();
            saved!.CardNumber.Should().Be("ТК-001");
            saved.DishName.Should().Be("Борщ");
            saved.Version.VersionNumber.Should().Be(1);
            saved.Version.OutputQuantity.Should().Be(250m);
            saved.Version.OutputUnitName.Should().Be("Грамм");
            saved.Version.Ingredients.Should().ContainSingle();
            saved.Version.Ingredients.Single().ProductName.Should().Be("Свёкла");
            saved.Version.Ingredients.Single().NetQuantity.Should().Be(80m);
        }

        [Fact]
        public async Task SaveAsync_WhenCardExists_ShouldUpdateVersionAndIngredients()
        {
            var id = await _service.SaveAsync(BuildModel("ТК-002", "Щи"));

            var saved = await _service.GetByIdAsync(id);
            var model = new RecipeCardDto
            {
                Id = id,
                CardNumber = "ТК-002",
                Name = "Щи из свежей капусты",
                DishId = _dishId,
                Version = new RecipeCardVersionDto
                {
                    Id = saved!.Version.Id,
                    VersionNumber = saved.Version.VersionNumber,
                    DateFrom = saved.Version.DateFrom,
                    OutputQuantity = 300m,
                    OutputUnitId = _unitId,
                    IsApproved = true,
                    Ingredients = new List<RecipeIngredientDto>
                    {
                        new()
                        {
                            Id = saved.Version.Ingredients.Single().Id,
                            ProductId = _productId,
                            UnitId = _unitId,
                            GrossQuantity = 120m,
                            NetQuantity = 100m,
                            LossPercent = 5m,
                            SortOrder = 1
                        }
                    }
                }
            };

            await _service.SaveAsync(model);

            var updated = await _service.GetByIdAsync(id);

            updated!.Name.Should().Be("Щи из свежей капусты");
            updated.Version.OutputQuantity.Should().Be(300m);
            updated.Version.IsApproved.Should().BeTrue();
            updated.Version.Ingredients.Single().NetQuantity.Should().Be(100m);
            updated.Version.Ingredients.Single().LossPercent.Should().Be(5m);
        }

        [Fact]
        public async Task SaveAsync_WhenCardNumberIsEmpty_ShouldThrow()
        {
            var model = BuildModel(" ", "Борщ");

            var act = async () => await _service.SaveAsync(model);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenOutputIsZero_ShouldThrow()
        {
            var model = BuildModel("ТК-003", "Борщ");
            model.Version.OutputQuantity = 0m;

            var act = async () => await _service.SaveAsync(model);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenDishDoesNotExist_ShouldThrow()
        {
            var model = BuildModel("ТК-004", "Борщ");
            model.DishId = Guid.NewGuid();

            var act = async () => await _service.SaveAsync(model);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenCardDoesNotExist_ShouldReturnNull()
        {
            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        /// <summary>Собирает модель новой техкарты с одной строкой ингредиента.</summary>
        private RecipeCardDto BuildModel(string cardNumber, string name) => new()
        {
            CardNumber = cardNumber,
            Name = name,
            DishId = _dishId,
            Version = new RecipeCardVersionDto
            {
                VersionNumber = 1,
                DateFrom = DateOnly.FromDateTime(DateTime.Today),
                OutputQuantity = 250m,
                OutputUnitId = _unitId,
                Ingredients = new List<RecipeIngredientDto>
                {
                    new()
                    {
                        ProductId = _productId,
                        UnitId = _unitId,
                        GrossQuantity = 100m,
                        NetQuantity = 80m,
                        LossPercent = 0m,
                        SortOrder = 1
                    }
                }
            }
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
