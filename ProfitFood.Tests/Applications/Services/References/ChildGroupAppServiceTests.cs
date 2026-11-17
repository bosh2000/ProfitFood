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
    public class ChildGroupAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IChildGroupRepository> _childGroupRepositoryMock;
        private readonly Mock<IAgeGroupRepository> _ageGroupRepositoryMock;

        public ChildGroupAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _childGroupRepositoryMock = new Mock<IChildGroupRepository>();
            _ageGroupRepositoryMock = new Mock<IAgeGroupRepository>();

            _dbRepositoryMock.Setup(x => x.childGroupRepository).Returns(_childGroupRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.ageGroupRepository).Returns(_ageGroupRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFillAgeGroupName()
        {
            var ageGroup = new AgeGroup { Name = "Младшая", AgeFromMonths = 12, AgeToMonths = 36 };

            _ageGroupRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<AgeGroup> { ageGroup });
            _childGroupRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<ChildGroup>
                {
                    new() { Name = "Группа 1", AgeGroupId = ageGroup.Id, PlannedChildrenCount = 20 }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().ContainSingle();
            result.Single().AgeGroupName.Should().Be("Младшая");
        }

        [Fact]
        public async Task GetAllAsync_WhenAgeGroupIsUnknown_ShouldReturnEmptyName()
        {
            _ageGroupRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<AgeGroup>());
            _childGroupRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<ChildGroup> { new() { Name = "Группа 2", AgeGroupId = Guid.NewGuid() } });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().AgeGroupName.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_WhenAgeGroupIsNotSelected_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new ChildGroupDto { Name = "Группа", AgeGroupId = Guid.Empty });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenAgeGroupDoesNotExist_ShouldThrowInvalidOperationException()
        {
            _ageGroupRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<AgeGroup, bool>>>()))
                .ReturnsAsync((AgeGroup)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new ChildGroupDto
            {
                Name = "Группа",
                AgeGroupId = Guid.NewGuid(),
                PlannedChildrenCount = 10
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenDataIsValid_ShouldCreateEntity()
        {
            var ageGroup = new AgeGroup { Name = "Младшая", AgeFromMonths = 12, AgeToMonths = 36 };

            _ageGroupRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<AgeGroup, bool>>>()))
                .ReturnsAsync(ageGroup);

            ChildGroup? created = null;
            _childGroupRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<ChildGroup>()))
                .Callback<ChildGroup>(entity => created = entity)
                .ReturnsAsync((ChildGroup entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new ChildGroupDto
            {
                Name = " Группа 3 ",
                AgeGroupId = ageGroup.Id,
                PlannedChildrenCount = 25
            });

            created.Should().NotBeNull();
            created!.Name.Should().Be("Группа 3");
            created.AgeGroupId.Should().Be(ageGroup.Id);
            created.PlannedChildrenCount.Should().Be(25);
        }

        private IChildGroupAppService CreateService()
            => new ChildGroupAppService(_dbRepositoryMock.Object, NullLogger<ChildGroupAppService>.Instance);
    }
}
