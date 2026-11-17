using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Dto.Documents;
using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Documents;
using ProfitFoot.Infrastructure;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Documents
{
    /// <summary>
    /// Проверяет проведение складских документов на реальной базе SQLite в памяти:
    /// движения товара нельзя проверить моками, они живут в SQL.
    /// </summary>
    public sealed class DocumentPostingTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProfitFoodDbContext> _options;
        private readonly DocumentAppService _service;
        private readonly Guid _locationId;
        private readonly Guid _baseUnitId;
        private readonly Guid _gramUnitId;
        private readonly Guid _productId;

        public DocumentPostingTests()
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
                // Идентификаторы генерируются в конструкторах сущностей, поэтому они читаются после сохранения.
                var baseUnit = new Unit
                {
                    Name = "Килограмм",
                    ShortName = "кг",
                    UnitType = UnitType.Weigth,
                    BaseFactor = 1m,
                    IsBase = true
                };

                var gramUnit = new Unit
                {
                    Name = "Грамм",
                    ShortName = "г",
                    UnitType = UnitType.Weigth,
                    BaseFactor = 0.001m,
                    IsBase = false
                };

                var location = new StorageLocation { Name = "Основной склад", Code = "01" };

                context.Units.AddRange(baseUnit, gramUnit);
                context.StorageLocations.Add(location);

                // Продукт ссылается на категорию обязательным ключом, без неё вставка падает по FK.
                var category = new ProductCategory { Name = "Молочные продукты" };
                context.ProductCategories.Add(category);

                context.SaveChanges();

                var product = new Product
                {
                    Name = "Молоко",
                    FullName = "Молоко 3,2%",
                    ProductCategoryId = category.Id,
                    BaseUnitId = baseUnit.Id,
                    MinStock = 1m
                };

                context.Products.Add(product);
                context.SaveChanges();

                _baseUnitId = baseUnit.Id;
                _gramUnitId = gramUnit.Id;
                _locationId = location.Id;
                _productId = product.Id;
            }

            var repositoryMock = new Mock<IDbRepository>();
            _service = new DocumentAppService(
                repositoryMock.Object,
                new TestContextFactory(_options),
                NullLogger<DocumentAppService>.Instance);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task Post_WhenReceipt_ShouldIncreaseBalance()
        {
            var documentId = CreateDraft(StockDocumentType.Receipt, quantity: 2m);

            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            Balance().Should().Be(2m);
            StatusOf(documentId).Should().Be(DocumentStatus.Posted);
        }

        [Fact]
        public async Task Post_WhenIssue_ShouldDecreaseBalance()
        {
            SeedBalance(5m);
            var documentId = CreateDraft(StockDocumentType.Issue, quantity: 3m);

            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            Balance().Should().Be(2m);
        }

        [Fact]
        public async Task Post_WhenIssueExceedsBalance_ShouldThrowAndKeepDraft()
        {
            SeedBalance(1m);
            var documentId = CreateDraft(StockDocumentType.Issue, quantity: 3m);

            var act = async () => await _service.PostAsync(new StockDocumentDto { Id = documentId });

            await act.Should().ThrowAsync<InvalidOperationException>();
            Balance().Should().Be(1m);
            StatusOf(documentId).Should().Be(DocumentStatus.Draft);
        }

        [Fact]
        public async Task Post_WhenWriteOff_ShouldDecreaseBalance()
        {
            SeedBalance(4m);
            var documentId = CreateDraft(StockDocumentType.WriteOff, quantity: 1.5m);

            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            Balance().Should().Be(2.5m);
        }

        [Fact]
        public async Task Post_WhenInventory_ShouldSetFactQuantity()
        {
            SeedBalance(5m);
            var documentId = CreateDraft(StockDocumentType.Inventory, quantity: 7m);

            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            Balance().Should().Be(7m);
        }

        [Fact]
        public async Task Post_WhenQuantityIsInGrams_ShouldConvertToBaseUnit()
        {
            var documentId = CreateDraft(StockDocumentType.Receipt, quantity: 500m, useGramUnit: true);

            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            // 500 г при BaseFactor = 0,001 дают 0,5 кг в базовой единице продукта.
            Balance().Should().Be(0.5m);
        }

        [Fact]
        public async Task Post_WhenDocumentHasNoItems_ShouldThrow()
        {
            var documentId = CreateEmptyDraft(StockDocumentType.Receipt);

            var act = async () => await _service.PostAsync(new StockDocumentDto { Id = documentId });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Unpost_WhenReceipt_ShouldReturnBalance()
        {
            var documentId = CreateDraft(StockDocumentType.Receipt, quantity: 2m);
            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            await _service.UnpostAsync(new StockDocumentDto { Id = documentId });

            Balance().Should().Be(0m);
            StatusOf(documentId).Should().Be(DocumentStatus.Draft);
        }

        [Fact]
        public async Task Unpost_WhenInventory_ShouldThrow()
        {
            SeedBalance(5m);
            var documentId = CreateDraft(StockDocumentType.Inventory, quantity: 7m);
            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            var act = async () => await _service.UnpostAsync(new StockDocumentDto { Id = documentId });

            await act.Should().ThrowAsync<InvalidOperationException>();
            Balance().Should().Be(7m);
        }

        [Fact]
        public async Task Save_WhenDocumentIsPosted_ShouldThrow()
        {
            var documentId = CreateDraft(StockDocumentType.Receipt, quantity: 2m);
            await _service.PostAsync(new StockDocumentDto { Id = documentId });

            var act = async () => await _service.SaveAsync(new StockDocumentDto
            {
                Id = documentId,
                DocumentType = StockDocumentType.Receipt,
                DocumentDate = DateOnly.FromDateTime(DateTime.Today),
                StorageLocationId = _locationId
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        /// <summary>Текущий остаток продукта на складе в базовой единице.</summary>
        private decimal Balance()
        {
            using var context = new ProfitFoodDbContext(_options);

            return context.StockBalances
                .Where(x => x.ProductId == _productId && x.StorageLocationId == _locationId)
                .Select(x => x.Quantity)
                .FirstOrDefault();
        }

        /// <summary>Статус документа.</summary>
        private DocumentStatus StatusOf(Guid documentId)
        {
            using var context = new ProfitFoodDbContext(_options);

            return context.StockDocuments
                .Where(x => x.Id == documentId)
                .Select(x => x.Status)
                .First();
        }

        /// <summary>Создаёт существующий остаток до проведения документа.</summary>
        private void SeedBalance(decimal quantity)
        {
            using var context = new ProfitFoodDbContext(_options);

            context.StockBalances.Add(new StockBalance
            {
                ProductId = _productId,
                StorageLocationId = _locationId,
                UnitId = _baseUnitId,
                Quantity = quantity,
                UpdatedAt = DateTime.Now
            });

            context.SaveChanges();
        }

        /// <summary>Создаёт черновик документа с одной строкой.</summary>
        private Guid CreateDraft(StockDocumentType documentType, decimal quantity, bool useGramUnit = false)
        {
            using var context = new ProfitFoodDbContext(_options);

            var document = new StockDocument
            {
                DocumentType = documentType,
                DocumentNumber = $"ТЕСТ-{Guid.NewGuid():N}",
                DocumentDate = DateOnly.FromDateTime(DateTime.Today),
                StorageLocationId = _locationId,
                Status = DocumentStatus.Draft,
                CreatedAt = DateTime.Now
            };

            context.StockDocuments.Add(document);
            context.StockDocumentItems.Add(new StockDocumentItem
            {
                StockDocumentId = document.Id,
                ProductId = _productId,
                UnitId = useGramUnit ? _gramUnitId : _baseUnitId,
                Quantity = quantity,
                Price = 100m,
                Amount = quantity * 100m
            });

            context.SaveChanges();

            return document.Id;
        }

        /// <summary>Создаёт черновик без строк — для проверки запрета проведения.</summary>
        private Guid CreateEmptyDraft(StockDocumentType documentType)
        {
            using var context = new ProfitFoodDbContext(_options);

            var document = new StockDocument
            {
                DocumentType = documentType,
                DocumentNumber = $"ТЕСТ-{Guid.NewGuid():N}",
                DocumentDate = DateOnly.FromDateTime(DateTime.Today),
                StorageLocationId = _locationId,
                Status = DocumentStatus.Draft,
                CreatedAt = DateTime.Now
            };

            context.StockDocuments.Add(document);
            context.SaveChanges();

            return document.Id;
        }

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
