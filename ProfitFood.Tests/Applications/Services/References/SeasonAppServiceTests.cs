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
    public class SeasonAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<ISeasonRepository> _repositoryMock;

        public SeasonAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _repositoryMock = new Mock<ISeasonRepository>();
            _dbRepositoryMock.Setup(x => x.seasonRepository).Returns(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapAndSortByMonthFrom()
        {
            _repositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<Season>
                {
                    new() { Name = "Лето", MonthFrom = 6, MonthTo = 8 },
                    new() { Name = "Зима", MonthFrom = 12, MonthTo = 2 }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.Name).Should().ContainInOrder("Лето", "Зима");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAll()
        {
            _repositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<Season> { new() { Name = "Осень", MonthFrom = 9, MonthTo = 11 } });

            var service = CreateService();

            var result = await service.SearchAsync(string.Empty);

            result.Should().HaveCount(1);
            _repositoryMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WhenMonthOutOfRange_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new SeasonDto { Name = "Кривой", MonthFrom = 0, MonthTo = 13 });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            Season? created = null;
            _repositoryMock
                .Setup(x => x.CreateASync(It.IsAny<Season>()))
                .Callback<Season>(entity => created = entity)
                .ReturnsAsync((Season entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new SeasonDto { Name = "Весна", MonthFrom = 3, MonthTo = 5 });

            created.Should().NotBeNull();
            created!.Name.Should().Be("Весна");
            created.MonthFrom.Should().Be(3);
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityExists_ShouldDelete()
        {
            var existing = new Season { Name = "Весна", MonthFrom = 3, MonthTo = 5 };

            _repositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Season, bool>>>()))
                .ReturnsAsync(existing);
            _repositoryMock
                .Setup(x => x.DeleteAsync(It.IsAny<Season>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.DeleteAsync(new SeasonDto { Id = existing.Id, Name = existing.Name });

            _repositoryMock.Verify(x => x.DeleteAsync(existing), Times.Once);
        }

        private ISeasonAppService CreateService()
            => new SeasonAppService(_dbRepositoryMock.Object, NullLogger<SeasonAppService>.Instance);
    }
}
