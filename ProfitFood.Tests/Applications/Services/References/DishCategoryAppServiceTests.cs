using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.References;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.References
{
    public class DishCategoryAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IDishCategoryRepository> _repositoryMock;

        public DishCategoryAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _repositoryMock = new Mock<IDishCategoryRepository>();
            _dbRepositoryMock.Setup(x => x.dishCategoryRepository).Returns(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapAndSortBySortOrder()
        {
            _repositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<DishCategory>
                {
                    new() { Name = "Напитки", SortOrder = 2 },
                    new() { Name = "Супы", SortOrder = 1 }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.Name).Should().ContainInOrder("Супы", "Напитки");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _repositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<DishCategory, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<DishCategory> { new() { Name = "Супы" } });

            var service = CreateService();

            var result = await service.SearchAsync("суп");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            DishCategory? created = null;
            _repositoryMock
                .Setup(x => x.CreateASync(It.IsAny<DishCategory>()))
                .Callback<DishCategory>(entity => created = entity)
                .ReturnsAsync((DishCategory entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new DishCategoryDto { Name = "Салаты", SortOrder = 3 });

            created.Should().NotBeNull();
            created!.Name.Should().Be("Салаты");
            created.SortOrder.Should().Be(3);
        }

        [Fact]
        public async Task SaveAsync_WhenNameIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new DishCategoryDto { Name = "  " });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenEntityExists_ShouldUpdate()
        {
            var existing = new DishCategory { Name = "Старое", SortOrder = 1 };

            _repositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DishCategory, bool>>>()))
                .ReturnsAsync(existing);
            _repositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<DishCategory>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.SaveAsync(new DishCategoryDto { Id = existing.Id, Name = "Новое", SortOrder = 5 });

            existing.Name.Should().Be("Новое");
            existing.SortOrder.Should().Be(5);
            _repositoryMock.Verify(x => x.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityExists_ShouldDelete()
        {
            var existing = new DishCategory { Name = "Салаты" };

            _repositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DishCategory, bool>>>()))
                .ReturnsAsync(existing);
            _repositoryMock
                .Setup(x => x.DeleteAsync(It.IsAny<DishCategory>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.DeleteAsync(new DishCategoryDto { Id = existing.Id, Name = existing.Name });

            _repositoryMock.Verify(x => x.DeleteAsync(existing), Times.Once);
        }

        private IDishCategoryAppService CreateService()
            => new DishCategoryAppService(_dbRepositoryMock.Object, NullLogger<DishCategoryAppService>.Instance);
    }
}
