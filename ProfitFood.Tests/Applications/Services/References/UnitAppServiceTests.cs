using AutoMapper;
using FluentAssertions;
using Moq;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.References;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.References
{
    public class UnitAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public UnitAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _mapperMock = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedUnits()
        {
            // Arrange
            var unit1 = new Unit { Name = "Килограмм" };
            var unit2 = new Unit { Name = "Грамм" };

            var entities = new List<Unit> { unit1, unit2 };

            _dbRepositoryMock
                .Setup(x => x.unitRepository.ToListAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(unit1))
                .Returns(new UnitItemDto { Id = unit1.Id, Name = unit1.Name });

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(unit2))
                .Returns(new UnitItemDto { Id = unit2.Id, Name = unit2.Name });

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(x => x.Name == "Килограмм");
            result.Should().Contain(x => x.Name == "Грамм");

            _dbRepositoryMock.Verify(x => x.unitRepository.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUnitDtos()
        {
            var units = new List<Unit>
            {
               new Unit{Name="Килограмм"},
               new Unit { Name = "Грамм" }
            };

            var dto1 = new UnitItemDto
            {
                Id = units[0].Id,
                Name = "Килограмм"
            };

            var dto2 = new UnitItemDto
            {
                Id = units[1].Id,
                Name = "Грамм"
            };

            _dbRepositoryMock
                .Setup(x => x.unitRepository.ToListAsync())
                .ReturnsAsync(units);

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(units[0]))
                .Returns(dto1);

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(units[1]))
                .Returns(dto2);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(x => x.Name == "Килограмм");
            result.Should().Contain(x => x.Name == "Грамм");

            _dbRepositoryMock.Verify(x => x.unitRepository.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_ShouldCreateUnit()
        {
            // Arrange
            var dto = new UnitItemDto
            {
                Id = Guid.NewGuid(),
                Name = "Литр"
            };

            var entity = new Unit
            {
                //     Id = dto.Id,
                Name = dto.Name
            };

            _mapperMock
                .Setup(x => x.Map<Unit>(dto))
                .Returns(entity);

            _dbRepositoryMock
                .Setup(x => x.unitRepository.CreateASync(entity))
                .ReturnsAsync(entity);

            var service = CreateService();

            // Act
            await service.SaveAsync(dto);

            // Assert
            _mapperMock.Verify(x => x.Map<Unit>(dto), Times.Once);
            _dbRepositoryMock.Verify(x => x.unitRepository.CreateASync(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteUnit()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UnitItemDto
            {
                Id = id,
                Name = "Штука"
            };

            var entity = new Unit
            {
                //     Id = id,
                Name = "Штука"
            };

            _dbRepositoryMock
                .Setup(x => x.unitRepository.FirstOfDefaultAsync(u => u.Id == dto.Id))
                .ReturnsAsync(entity);

            _dbRepositoryMock
                .Setup(x => x.unitRepository.DeleteAsync(entity))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(dto);

            // Assert
            _dbRepositoryMock.Verify(
                x => x.unitRepository.DeleteAsync(entity),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenUnitExists_ShouldDeleteUnit()
        {
            // Arrange
            var dto = new UnitItemDto
            {
                Id = Guid.NewGuid(),
                Name = "Штука"
            };

            var entity = new Unit
            {
                Name = dto.Name
            };

            _dbRepositoryMock
                .Setup(x => x.unitRepository.FirstOfDefaultAsync(u => u.Id == dto.Id))
                .ReturnsAsync(entity);

            _dbRepositoryMock
                .Setup(x => x.unitRepository.DeleteAsync(entity))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(dto);

            // Assert
            _dbRepositoryMock.Verify(x => x.unitRepository.DeleteAsync(entity), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAllUnits()
        {
            var gram = new Unit { Name = "Грамм" };

            _dbRepositoryMock
                .Setup(x => x.unitRepository.ToListAsync())
                .ReturnsAsync(new List<Unit> { gram });

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(gram))
                .Returns(new UnitItemDto { Id = gram.Id, Name = gram.Name });

            var service = CreateService();

            var result = await service.SearchAsync(" ");

            result.Should().HaveCount(1);
            result.Single().Name.Should().Be("Грамм");

            _dbRepositoryMock.Verify(x => x.unitRepository.ToListAsync(), Times.Once);
        }

        public async Task SearchAsync_WhenSearchTextProvided_ShouldReturnFilteredUnits()
        {
            var gram = new Unit { Name = "Грамм" };
            var kilogram = new Unit { Name = "Килограмм" };
            _dbRepositoryMock
                .Setup(x => x.unitRepository.ConditionToListAsync(
                    It.IsAny<Expression<Func<Unit, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<Unit> { gram, kilogram });

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(gram))
                .Returns(new UnitItemDto { Id = gram.Id, Name = gram.Name });

            _mapperMock
                .Setup(x => x.Map<UnitItemDto>(kilogram))
                .Returns(new UnitItemDto { Id = kilogram.Id, Name = kilogram.Name });

            var service = CreateService();

            var result = await service.SearchAsync("гр");

            result.Should().HaveCount(2);
            result.Select(x => x.Name).Should().ContainInOrder("Грамм", "Килограмм");

            _dbRepositoryMock.Verify(
                x => x.unitRepository.ConditionToListAsync(It.IsAny<Expression<Func<Unit, bool>>>(), It.IsAny<bool>()),
                Times.Once);
        }

        // пустая модель
        [Fact]
        public async Task DeleteAsyncNullModel_ShouldThrowArgumentNullException()
        {
            var service = CreateService();
            var act = async () => await service.DeleteAsync(null);
            await act.Should().ThrowAsync<ArgumentNullException>("UnitItemDto");
        }

        // не существующий в БД Unit
        [Fact]
        public async Task DeleteAsyncNotFoundUnit_ShowThrowAgrumentNullException()
        {
            var dto = new UnitItemDto
            {
                Id = Guid.NewGuid(),
                Name = "Штука"
            };
            Unit entity = null;
            _dbRepositoryMock
                .Setup(x => x.unitRepository.FirstOfDefaultAsync(u => u.Id == dto.Id))
                .ReturnsAsync((Unit)null);

            _dbRepositoryMock
                   .Setup(x => x.unitRepository.DeleteAsync(entity))
                   .Returns(Task.CompletedTask);
            var service = CreateService();
            var act = async () => await service.DeleteAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>("Единица измерения не найдена.");
        }

        private UnitAppService CreateService()
        {
            return new UnitAppService(
                _dbRepositoryMock.Object,
                _mapperMock.Object);
        }
    }
}