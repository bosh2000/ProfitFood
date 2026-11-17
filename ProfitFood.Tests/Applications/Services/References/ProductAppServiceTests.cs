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
    public class ProductAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IProductCategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IUnitRepository> _unitRepositoryMock;

        public ProductAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _categoryRepositoryMock = new Mock<IProductCategoryRepository>();
            _unitRepositoryMock = new Mock<IUnitRepository>();

            _dbRepositoryMock.Setup(x => x.productRepository).Returns(_productRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.productCategoryRepository).Returns(_categoryRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(_unitRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFillCategoryAndUnitNames()
        {
            var category = new ProductCategory { Name = "Молочные" };
            var baseUnit = new Unit { Name = "Килограмм", ShortName = "кг" };

            _categoryRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<ProductCategory> { category });
            _unitRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit> { baseUnit });
            _productRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<Product>
                {
                    new()
                    {
                        Name = "Молоко",
                        FullName = "Молоко 3,2%",
                        ProductCategoryId = category.Id,
                        BaseUnitId = baseUnit.Id,
                        MinStock = 5m
                    }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var product = result.Should().ContainSingle().Subject;
            product.ProductCategoryName.Should().Be("Молочные");
            product.BaseUnitName.Should().Be("Килограмм");
            product.StorageUnitName.Should().BeEmpty();
            product.MinStock.Should().Be(5m);
        }

        [Fact]
        public async Task SaveAsync_WhenNameIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new ProductDto { Name = "  " });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenCategoryIsNotSelected_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new ProductDto
            {
                Name = "Молоко",
                ProductCategoryId = Guid.Empty,
                BaseUnitId = Guid.NewGuid()
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenCategoryDoesNotExist_ShouldThrowInvalidOperationException()
        {
            _categoryRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>()))
                .ReturnsAsync((ProductCategory)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new ProductDto
            {
                Name = "Молоко",
                ProductCategoryId = Guid.NewGuid(),
                BaseUnitId = Guid.NewGuid()
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenDataIsValid_ShouldCreateEntity()
        {
            var category = new ProductCategory { Name = "Молочные" };
            var unit = new Unit { Name = "Литр", ShortName = "л" };

            _categoryRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>()))
                .ReturnsAsync(category);
            _unitRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Unit, bool>>>()))
                .ReturnsAsync(unit);

            Product? created = null;
            _productRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<Product>()))
                .Callback<Product>(entity => created = entity)
                .ReturnsAsync((Product entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new ProductDto
            {
                Name = " Молоко ",
                FullName = " Молоко 3,2% ",
                Article = " ",
                ProductCategoryId = category.Id,
                BaseUnitId = unit.Id,
                MinStock = 3m,
                IsPerishable = true
            });

            created.Should().NotBeNull();
            created!.Name.Should().Be("Молоко");
            created.FullName.Should().Be("Молоко 3,2%");
            created.Article.Should().BeNull();
            created.StorageUnitId.Should().BeNull();
            created.MinStock.Should().Be(3m);
            created.IsPerishable.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _productRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync((Product)null!);

            var service = CreateService();

            var act = async () => await service.DeleteAsync(new ProductDto { Id = Guid.NewGuid(), Name = "Нет" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IProductAppService CreateService()
            => new ProductAppService(_dbRepositoryMock.Object, NullLogger<ProductAppService>.Instance);
    }
}
