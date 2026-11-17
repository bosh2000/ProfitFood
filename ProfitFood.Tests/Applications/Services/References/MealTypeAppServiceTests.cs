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
    public class MealTypeAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IMealTypeRepository> _repositoryMock;

        public MealTypeAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _repositoryMock = new Mock<IMealTypeRepository>();
            _dbRepositoryMock.Setup(x => x.mealTypeRepository).Returns(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapAndSortBySortOrder()
        {
            _repositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<MealType>
                {
                    new() { Name = "Ужин", Code = "S", SortOrder = 3 },
                    new() { Name = "Завтрак", Code = "B", SortOrder = 1 }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.Code).Should().ContainInOrder("B", "S");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _repositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<MealType, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<MealType> { new() { Name = "Обед", Code = "L", SortOrder = 2 } });

            var service = CreateService();

            var result = await service.SearchAsync("обед");

            result.Should().ContainSingle(x => x.Code == "L");
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            MealType? created = null;
            _repositoryMock
                .Setup(x => x.CreateASync(It.IsAny<MealType>()))
                .Callback<MealType>(entity => created = entity)
                .ReturnsAsync((MealType entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new MealTypeDto { Name = "Полдник", Code = "K", SortOrder = 4 });

            created.Should().NotBeNull();
            created!.Code.Should().Be("K");
            created.SortOrder.Should().Be(4);
        }

        [Fact]
        public async Task SaveAsync_WhenCodeIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new MealTypeDto { Name = "Обед", Code = " " });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _repositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<MealType, bool>>>()))
                .ReturnsAsync((MealType)null!);

            var service = CreateService();

            var act = async () => await service.DeleteAsync(new MealTypeDto { Id = Guid.NewGuid(), Name = "Нет", Code = "X" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IMealTypeAppService CreateService()
            => new MealTypeAppService(_dbRepositoryMock.Object, NullLogger<MealTypeAppService>.Instance);
    }
}
