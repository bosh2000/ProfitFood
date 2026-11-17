using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Documents;
using Microsoft.EntityFrameworkCore;
using ProfitFoot.Infrastructure;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Documents
{
    public class DocumentAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IStockDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IStorageLocationRepository> _locationRepositoryMock;
        private readonly Mock<IDbContextFactory<ProfitFoodDbContext>> _contextFactoryMock;

        public DocumentAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _documentRepositoryMock = new Mock<IStockDocumentRepository>();
            _locationRepositoryMock = new Mock<IStorageLocationRepository>();

            _dbRepositoryMock.Setup(x => x.stockDocumentRepository).Returns(_documentRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.storageLocationRepository).Returns(_locationRepositoryMock.Object);
            _contextFactoryMock = new Mock<IDbContextFactory<ProfitFoodDbContext>>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldMapTypeStatusAndLocation()
        {
            var location = new StorageLocation { Name = "Овощной склад", Code = "02" };
            var document = new StockDocument
            {
                DocumentNumber = "ПН-001",
                DocumentDate = new DateOnly(2026, 3, 12),
                DocumentType = StockDocumentType.Receipt,
                Status = DocumentStatus.Posted,
                StorageLocationId = location.Id
            };

            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation> { location });
            _documentRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StockDocument> { document });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var item = result.Should().ContainSingle().Subject;
            item.DocumentNumber.Should().Be("ПН-001");
            item.DocumentTypeName.Should().Be(nameof(StockDocumentType.Receipt));
            item.StatusName.Should().Be(nameof(DocumentStatus.Posted));
            item.StorageLocationName.Should().Be("Овощной склад");
            item.DocumentDate.Should().Be(new DateOnly(2026, 3, 12));
        }

        [Fact]
        public async Task GetAllAsync_WhenLocationIsUnknown_ShouldReturnEmptyName()
        {
            var document = new StockDocument
            {
                DocumentNumber = "ВД-002",
                DocumentDate = new DateOnly(2026, 3, 13),
                StorageLocationId = Guid.NewGuid()
            };

            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _documentRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StockDocument> { document });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().StorageLocationName.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByDocumentDateDescending()
        {
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _documentRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockDocument>
                {
                    new() { DocumentNumber = "1", DocumentDate = new DateOnly(2026, 3, 10) },
                    new() { DocumentNumber = "3", DocumentDate = new DateOnly(2026, 3, 12) },
                    new() { DocumentNumber = "2", DocumentDate = new DateOnly(2026, 3, 11) }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.DocumentNumber).Should().ContainInOrder("3", "2", "1");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAll()
        {
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _documentRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockDocument> { new() { DocumentNumber = "ПН-001" } });

            var service = CreateService();

            var result = await service.SearchAsync("   ");

            result.Should().HaveCount(1);
            _documentRepositoryMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _documentRepositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<StockDocument, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<StockDocument> { new() { DocumentNumber = "ПН-001" } });

            var service = CreateService();

            var result = await service.SearchAsync("ПН");

            result.Should().HaveCount(1);
            _documentRepositoryMock.Verify(
                x => x.ConditionToListAsync(It.IsAny<Expression<Func<StockDocument, bool>>>(), It.IsAny<bool>()),
                Times.Once);
        }

        private IDocumentAppService CreateService()
            => new DocumentAppService(_dbRepositoryMock.Object, _contextFactoryMock.Object, NullLogger<DocumentAppService>.Instance);
    }
}
