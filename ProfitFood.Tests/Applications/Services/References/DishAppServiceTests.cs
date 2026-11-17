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
    public class DishAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IDishRepository> _dishRepositoryMock;
        private readonly Mock<IDishCategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMealTypeRepository> _mealTypeRepositoryMock;

        public DishAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _dishRepositoryMock = new Mock<IDishRepository>();
            _categoryRepositoryMock = new Mock<IDishCategoryRepository>();
            _mealTypeRepositoryMock = new Mock<IMealTypeRepository>();

            _dbRepositoryMock.Setup(x => x.dishRepository).Returns(_dishRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.dishCategoryRepository).Returns(_categoryRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.mealTypeRepository).Returns(_mealTypeRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFillCategoryAndMealTypeNames()
        {
            var category = new DishCategory { Name = "Супы", SortOrder = 1 };
            var mealType = new MealType { Name = "Обед", Code = "L", SortOrder = 2 };

            _categoryRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<DishCategory> { category });
            _mealTypeRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<MealType> { mealType });
            _dishRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<Dish>
                {
                    new()
                    {
                        Name = "Борщ",
                        Code = "С-001",
                        DishCategoryId = category.Id,
                        MealTypeId = mealType.Id
                    }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var dish = result.Should().ContainSingle().Subject;
            dish.DishCategoryName.Should().Be("Супы");
            dish.MealTypeName.Should().Be("Обед");
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByName()
        {
            _categoryRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<DishCategory>());
            _mealTypeRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<MealType>());
            _dishRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<Dish>
                {
                    new() { Name = "Щи", Code = "С-002" },
                    new() { Name = "Борщ", Code = "С-001" }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.Name).Should().ContainInOrder("Борщ", "Щи");
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateEntity()
        {
            var category = new DishCategory { Name = "Супы" };
            var mealType = new MealType { Name = "Обед", Code = "L" };

            _categoryRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DishCategory, bool>>>()))
                .ReturnsAsync(category);
            _mealTypeRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<MealType, bool>>>()))
                .ReturnsAsync(mealType);

            Dish? created = null;
            _dishRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<Dish>()))
                .Callback<Dish>(entity => created = entity)
                .ReturnsAsync((Dish entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new DishDto
            {
                Name = " Борщ ",
                Code = " С-001 ",
                DishCategoryId = category.Id,
                MealTypeId = mealType.Id,
                Notes = "  со сметаной  "
            });

            created.Should().NotBeNull();
            created!.Name.Should().Be("Борщ");
            created.Code.Should().Be("С-001");
            created.Notes.Should().Be("со сметаной");
            created.DishCategoryId.Should().Be(category.Id);
        }

        [Fact]
        public async Task SaveAsync_WhenNameIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new DishDto { Name = " ", Code = "С-001" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenCodeIsEmpty_ShouldThrowInvalidOperationException()
        {
            var service = CreateService();

            var act = async () => await service.SaveAsync(new DishDto { Name = "Борщ", Code = "  " });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenCategoryDoesNotExist_ShouldThrowInvalidOperationException()
        {
            _categoryRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DishCategory, bool>>>()))
                .ReturnsAsync((DishCategory)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new DishDto
            {
                Name = "Борщ",
                Code = "С-001",
                DishCategoryId = Guid.NewGuid(),
                MealTypeId = Guid.NewGuid()
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _dishRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Dish, bool>>>()))
                .ReturnsAsync((Dish)null!);

            var service = CreateService();

            var act = async () => await service.DeleteAsync(new DishDto { Id = Guid.NewGuid(), Name = "Нет" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IDishAppService CreateService()
            => new DishAppService(_dbRepositoryMock.Object, NullLogger<DishAppService>.Instance);
    }
}
