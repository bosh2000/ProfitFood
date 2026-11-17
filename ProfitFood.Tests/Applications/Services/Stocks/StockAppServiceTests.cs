using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Applications.Dto.Stocks;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Stocks;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Stocks
{
    public class StockAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IStockBalanceRepository> _balanceRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IStorageLocationRepository> _locationRepositoryMock;
        private readonly Mock<IUnitRepository> _unitRepositoryMock;

        public StockAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _balanceRepositoryMock = new Mock<IStockBalanceRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _locationRepositoryMock = new Mock<IStorageLocationRepository>();
            _unitRepositoryMock = new Mock<IUnitRepository>();

            _dbRepositoryMock.Setup(x => x.stockBalanceRepository).Returns(_balanceRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.productRepository).Returns(_productRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.storageLocationRepository).Returns(_locationRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(_unitRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFillProductLocationAndUnitNames()
        {
            var product = new Product { Name = "Молоко 3,2%", MinStock = 5m };
            var location = new StorageLocation { Name = "Холодильный склад", Code = "01" };
            var unit = new Unit { Name = "Литр", ShortName = "л" };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { product });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation> { location });
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit> { unit });
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance>
                {
                    new()
                    {
                        ProductId = product.Id,
                        StorageLocationId = location.Id,
                        UnitId = unit.Id,
                        Quantity = 12m
                    }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var item = result.Should().ContainSingle().Subject;
            item.ProductName.Should().Be("Молоко 3,2%");
            item.StorageLocationName.Should().Be("Холодильный склад");
            item.UnitName.Should().Be("л");
            item.Quantity.Should().Be(12m);
            item.MinStock.Should().Be(5m);
        }

        [Fact]
        public async Task GetAllAsync_WhenQuantityBelowMinStock_ShouldMarkDeficit()
        {
            var product = new Product { Name = "Масло сливочное", MinStock = 5m };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { product });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance> { new() { ProductId = product.Id, Quantity = 2m } });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().IsDeficit.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WhenQuantityEqualsMinStock_ShouldNotMarkDeficit()
        {
            var product = new Product { Name = "Сахар", MinStock = 5m };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { product });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance> { new() { ProductId = product.Id, Quantity = 5m } });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().IsDeficit.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_WhenProductIsUnknown_ShouldReturnEmptyName()
        {
            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product>());
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance> { new() { ProductId = Guid.NewGuid(), Quantity = 1m } });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var item = result.Single();
            item.ProductName.Should().BeEmpty();
            item.MinStock.Should().Be(0m);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByProductName()
        {
            var salt = new Product { Name = "Соль", MinStock = 1m };
            var milk = new Product { Name = "Молоко", MinStock = 1m };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { salt, milk });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance>
                {
                    new() { ProductId = salt.Id, Quantity = 3m },
                    new() { ProductId = milk.Id, Quantity = 3m }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.ProductName).Should().ContainInOrder("Молоко", "Соль");
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAll()
        {
            var product = new Product { Name = "Молоко", MinStock = 1m };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { product });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance> { new() { ProductId = product.Id, Quantity = 1m } });

            var service = CreateService();

            var result = await service.SearchAsync(" ");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task SearchAsync_ShouldFilterByProductName()
        {
            var milk = new Product { Name = "Молоко", MinStock = 1m };
            var bread = new Product { Name = "Хлеб", MinStock = 1m };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { milk, bread });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation>());
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance>
                {
                    new() { ProductId = milk.Id, Quantity = 1m },
                    new() { ProductId = bread.Id, Quantity = 1m }
                });

            var service = CreateService();

            var result = await service.SearchAsync("мол");

            result.Should().ContainSingle(x => x.ProductName == "Молоко");
        }

        [Fact]
        public async Task SearchAsync_ShouldFilterByStorageLocationName()
        {
            var product = new Product { Name = "Молоко", MinStock = 1m };
            var location = new StorageLocation { Name = "Холодильный склад", Code = "01" };

            _productRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Product> { product });
            _locationRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<StorageLocation> { location });
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit>());
            _balanceRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<StockBalance>
                {
                    new() { ProductId = product.Id, StorageLocationId = location.Id, Quantity = 1m }
                });

            var service = CreateService();

            var result = await service.SearchAsync("холод");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            var product = new Product { Name = "Молоко", MinStock = 1m };
            var location = new StorageLocation { Name = "Холодильный склад", Code = "01" };
            var unit = new Unit { Name = "Литр", ShortName = "л" };

            _productRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(product);
            _locationRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<StorageLocation, bool>>>()))
                .ReturnsAsync(location);
            _unitRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Unit, bool>>>()))
                .ReturnsAsync(unit);

            StockBalance? created = null;
            _balanceRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<StockBalance>()))
                .Callback<StockBalance>(entity => created = entity)
                .ReturnsAsync((StockBalance entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new StockBalanceDto
            {
                ProductId = product.Id,
                StorageLocationId = location.Id,
                UnitId = unit.Id,
                Quantity = 7m
            });

            created.Should().NotBeNull();
            created!.ProductId.Should().Be(product.Id);
            created.StorageLocationId.Should().Be(location.Id);
            created.UnitId.Should().Be(unit.Id);
            created.Quantity.Should().Be(7m);
        }

        [Fact]
        public async Task SaveAsync_WhenEntityExists_ShouldUpdate()
        {
            var product = new Product { Name = "Молоко", MinStock = 1m };
            var location = new StorageLocation { Name = "Склад", Code = "01" };
            var unit = new Unit { Name = "Литр", ShortName = "л" };
            var balance = new StockBalance
            {
                ProductId = product.Id,
                StorageLocationId = location.Id,
                UnitId = unit.Id,
                Quantity = 2m
            };

            _productRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(product);
            _locationRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<StorageLocation, bool>>>()))
                .ReturnsAsync(location);
            _unitRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Unit, bool>>>()))
                .ReturnsAsync(unit);
            _balanceRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<StockBalance, bool>>>()))
                .ReturnsAsync(balance);
            _balanceRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<StockBalance>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.SaveAsync(new StockBalanceDto
            {
                Id = balance.Id,
                ProductId = product.Id,
                StorageLocationId = location.Id,
                UnitId = unit.Id,
                Quantity = 15m
            });

            balance.Quantity.Should().Be(15m);
            _balanceRepositoryMock.Verify(x => x.UpdateAsync(balance), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WhenQuantityIsNegative_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new StockBalanceDto
            {
                ProductId = Guid.NewGuid(),
                StorageLocationId = Guid.NewGuid(),
                UnitId = Guid.NewGuid(),
                Quantity = -1m
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenProductIsNotSelected_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new StockBalanceDto
            {
                ProductId = Guid.Empty,
                StorageLocationId = Guid.NewGuid(),
                UnitId = Guid.NewGuid(),
                Quantity = 1m
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenProductDoesNotExist_ShouldThrowInvalidOperationException()
        {
            _productRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync((Product)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new StockBalanceDto
            {
                ProductId = Guid.NewGuid(),
                StorageLocationId = Guid.NewGuid(),
                UnitId = Guid.NewGuid(),
                Quantity = 1m
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenUnitDoesNotExist_ShouldThrowInvalidOperationException()
        {
            var product = new Product { Name = "Молоко", MinStock = 1m };
            var location = new StorageLocation { Name = "Склад", Code = "01" };

            _productRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(product);
            _locationRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<StorageLocation, bool>>>()))
                .ReturnsAsync(location);
            _unitRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Unit, bool>>>()))
                .ReturnsAsync((Unit)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new StockBalanceDto
            {
                ProductId = product.Id,
                StorageLocationId = location.Id,
                UnitId = Guid.NewGuid(),
                Quantity = 1m
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IStockAppService CreateService()
            => new StockAppService(_dbRepositoryMock.Object, NullLogger<StockAppService>.Instance);
    }
}
