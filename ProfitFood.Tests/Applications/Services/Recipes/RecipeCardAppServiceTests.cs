using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Recipes;
using Microsoft.EntityFrameworkCore;
using ProfitFoot.Infrastructure;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Recipes
{
    public class RecipeCardAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IRecipeCardRepository> _recipeCardRepositoryMock;
        private readonly Mock<IRecipeCardVersionRepository> _versionRepositoryMock;
        private readonly Mock<IUnitRepository> _unitRepositoryMock;
        private readonly Mock<IDbContextFactory<ProfitFoodDbContext>> _contextFactoryMock;

        public RecipeCardAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _recipeCardRepositoryMock = new Mock<IRecipeCardRepository>();
            _versionRepositoryMock = new Mock<IRecipeCardVersionRepository>();
            _unitRepositoryMock = new Mock<IUnitRepository>();

            _dbRepositoryMock.Setup(x => x.recipeCardRepository).Returns(_recipeCardRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.recipeCardVersionRepository).Returns(_versionRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(_unitRepositoryMock.Object);
            _contextFactoryMock = new Mock<IDbContextFactory<ProfitFoodDbContext>>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldUseLatestVersion()
        {
            var unit = new Unit { Name = "Грамм", ShortName = "г" };
            var card = new RecipeCard { CardNumber = "ТК-001", Name = "Каша молочная" };

            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit> { unit });
            _recipeCardRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCard> { card });
            _versionRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<RecipeCardVersion>
                {
                    new()
                    {
                        RecipeCardId = card.Id,
                        VersionNumber = 1,
                        DateFrom = new DateOnly(2025, 1, 1),
                        OutputQuantity = 180m,
                        OutputUnitId = unit.Id,
                        IsApproved = false
                    },
                    new()
                    {
                        RecipeCardId = card.Id,
                        VersionNumber = 2,
                        DateFrom = new DateOnly(2026, 1, 1),
                        OutputQuantity = 200m,
                        OutputUnitId = unit.Id,
                        IsApproved = true
                    }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var item = result.Should().ContainSingle().Subject;
            item.VersionNumber.Should().Be(2);
            item.OutputQuantity.Should().Be(200m);
            item.IsApproved.Should().BeTrue();
            item.VersionDateFrom.Should().Be(new DateOnly(2026, 1, 1));
        }

        [Fact]
        public async Task GetAllAsync_ShouldFillOutputUnitName()
        {
            var unit = new Unit { Name = "Грамм", ShortName = "г" };
            var card = new RecipeCard { CardNumber = "ТК-002", Name = "Суп овощной" };

            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit> { unit });
            _recipeCardRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCard> { card });
            _versionRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<RecipeCardVersion>
                {
                    new()
                    {
                        RecipeCardId = card.Id,
                        VersionNumber = 1,
                        DateFrom = new DateOnly(2026, 1, 1),
                        OutputQuantity = 250m,
                        OutputUnitId = unit.Id
                    }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().OutputUnitName.Should().Be("Грамм");
        }

        [Fact]
        public async Task GetAllAsync_WhenVersionIsMissing_ShouldReturnEmptyVersionFields()
        {
            var card = new RecipeCard { CardNumber = "ТК-003", Name = "Без версии" };

            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _recipeCardRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCard> { card });
            _versionRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCardVersion>());

            var service = CreateService();

            var result = await service.GetAllAsync();

            var item = result.Single();
            item.VersionNumber.Should().BeNull();
            item.OutputQuantity.Should().BeNull();
            item.OutputUnitName.Should().BeEmpty();
            item.IsApproved.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByCardNumber()
        {
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _versionRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCardVersion>());
            _recipeCardRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<RecipeCard>
                {
                    new() { CardNumber = "ТК-020", Name = "Второе" },
                    new() { CardNumber = "ТК-005", Name = "Первое" }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.CardNumber).Should().ContainInOrder("ТК-005", "ТК-020");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAll()
        {
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _versionRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCardVersion>());
            _recipeCardRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<RecipeCard> { new() { CardNumber = "ТК-001", Name = "Каша" } });

            var service = CreateService();

            var result = await service.SearchAsync(string.Empty);

            result.Should().HaveCount(1);
            _recipeCardRepositoryMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _versionRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<RecipeCardVersion>());
            _recipeCardRepositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<RecipeCard, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<RecipeCard> { new() { CardNumber = "ТК-001", Name = "Каша молочная" } });

            var service = CreateService();

            var result = await service.SearchAsync("каша");

            result.Should().HaveCount(1);
            _recipeCardRepositoryMock.Verify(
                x => x.ConditionToListAsync(It.IsAny<Expression<Func<RecipeCard, bool>>>(), It.IsAny<bool>()),
                Times.Once);
        }

        private IRecipeCardAppService CreateService()
            => new RecipeCardAppService(
                _dbRepositoryMock.Object,
                _contextFactoryMock.Object,
                NullLogger<RecipeCardAppService>.Instance);
    }
}
