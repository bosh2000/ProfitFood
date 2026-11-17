using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.References;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.References
{
    public class AgeGroupAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IAgeGroupRepository> _ageGroupRepositoryMock;

        public AgeGroupAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _ageGroupRepositoryMock = new Mock<IAgeGroupRepository>();
            _dbRepositoryMock.Setup(x => x.ageGroupRepository).Returns(_ageGroupRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapAndSortByAgeFrom()
        {
            var older = new AgeGroup { Name = "Старшая", AgeFromMonths = 60, AgeToMonths = 84 };
            var younger = new AgeGroup { Name = "Младшая", AgeFromMonths = 12, AgeToMonths = 36 };

            _ageGroupRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<AgeGroup> { older, younger });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
            result.Select(x => x.Name).Should().ContainInOrder("Младшая", "Старшая");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAll()
        {
            _ageGroupRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<AgeGroup> { new() { Name = "Средняя", AgeFromMonths = 36, AgeToMonths = 60 } });

            var service = CreateService();

            var result = await service.SearchAsync(" ");

            result.Should().HaveCount(1);
            _ageGroupRepositoryMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _ageGroupRepositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<AgeGroup, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<AgeGroup> { new() { Name = "Младшая", AgeFromMonths = 12, AgeToMonths = 36 } });

            var service = CreateService();

            var result = await service.SearchAsync("млад");

            result.Should().HaveCount(1);
            _ageGroupRepositoryMock.Verify(
                x => x.ConditionToListAsync(It.IsAny<Expression<Func<AgeGroup, bool>>>(), It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            var dto = new ProfitFood.Applications.Dto.References.AgeGroupDto
            {
                Name = " Ясельная ",
                AgeFromMonths = 6,
                AgeToMonths = 24
            };

            AgeGroup? created = null;
            _ageGroupRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<AgeGroup>()))
                .Callback<AgeGroup>(entity => created = entity)
                .ReturnsAsync((AgeGroup entity) => entity);

            var service = CreateService();

            await service.SaveAsync(dto);

            created.Should().NotBeNull();
            created!.Name.Should().Be("Ясельная");
            created.AgeFromMonths.Should().Be(6);
            created.AgeToMonths.Should().Be(24);
        }

        [Fact]
        public async Task SaveAsync_WhenNameIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new ProfitFood.Applications.Dto.References.AgeGroupDto { Name = " " });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenAgeRangeIsInvalid_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new ProfitFood.Applications.Dto.References.AgeGroupDto
            {
                Name = "Некорректная",
                AgeFromMonths = 60,
                AgeToMonths = 12
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _ageGroupRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<AgeGroup, bool>>>()))
                .ReturnsAsync((AgeGroup)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new ProfitFood.Applications.Dto.References.AgeGroupDto
            {
                Id = Guid.NewGuid(),
                Name = "Пропавшая",
                AgeFromMonths = 12,
                AgeToMonths = 36
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _ageGroupRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<AgeGroup, bool>>>()))
                .ReturnsAsync((AgeGroup)null!);

            var service = CreateService();

            var act = async () => await service.DeleteAsync(
                new ProfitFood.Applications.Dto.References.AgeGroupDto { Id = Guid.NewGuid(), Name = "Нет" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IAgeGroupAppService CreateService()
            => new AgeGroupAppService(_dbRepositoryMock.Object, NullLogger<AgeGroupAppService>.Instance);
    }
}
