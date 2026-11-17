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
    public class StorageLocationAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IStorageLocationRepository> _repositoryMock;

        public StorageLocationAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _repositoryMock = new Mock<IStorageLocationRepository>();
            _dbRepositoryMock.Setup(x => x.storageLocationRepository).Returns(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapAndSortByName()
        {
            _repositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StorageLocation>
                {
                    new() { Name = "Овощной склад", Code = "02" },
                    new() { Name = "Аптека", Code = "01" }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.Name).Should().ContainInOrder("Аптека", "Овощной склад");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _repositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<StorageLocation, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<StorageLocation> { new() { Name = "Аптека", Code = "01" } });

            var service = CreateService();

            var result = await service.SearchAsync("апт");

            result.Should().ContainSingle(x => x.Code == "01");
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            StorageLocation? created = null;
            _repositoryMock
                .Setup(x => x.CreateASync(It.IsAny<StorageLocation>()))
                .Callback<StorageLocation>(entity => created = entity)
                .ReturnsAsync((StorageLocation entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new StorageLocationDto { Name = " Мясной ", Code = " 03 " });

            created.Should().NotBeNull();
            created!.Name.Should().Be("Мясной");
            created.Code.Should().Be("03");
        }

        [Fact]
        public async Task SaveAsync_WhenCodeIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new StorageLocationDto { Name = "Склад", Code = "  " });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _repositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<StorageLocation, bool>>>()))
                .ReturnsAsync((StorageLocation)null!);

            var service = CreateService();

            var act = async () => await service.DeleteAsync(new StorageLocationDto { Id = Guid.NewGuid(), Name = "Нет", Code = "99" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IStorageLocationAppService CreateService()
            => new StorageLocationAppService(_dbRepositoryMock.Object, NullLogger<StorageLocationAppService>.Instance);
    }
}
