using AutoMapper;
using FluentAssertions;
using Moq;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.References;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.DAL.Repository.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        // Тест проверяет, что сервис корректно получает все сущности из репозитория,
        // маппит их в DTO и возвращает отсортированный список.
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedUnits()
        {
            // Arrange: подготовка данных и моков
            var unit1 = new Unit { Name = "Килограмм" };
            var unit2 = new Unit { Name = "Грамм" };

            var entities = new List<Unit> { unit1, unit2 };

            var unitRepoMock = new Mock<IUnitRepository>();
            unitRepoMock.Setup(x => x.ToListAsync())
                        .ReturnsAsync(entities);
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(unitRepoMock.Object);

            // Маппер конвертирует сущности в DTO
            _mapperMock.Setup(x => x.Map<UnitItemDto>(unit1))
                       .Returns(new UnitItemDto { Id = unit1.Id, Name = unit1.Name });

            _mapperMock.Setup(x => x.Map<UnitItemDto>(unit2))
                       .Returns(new UnitItemDto { Id = unit2.Id, Name = unit2.Name });

            var service = CreateService();

            // Act: вызов тестируемого метода
            var result = await service.GetAllAsync();

            // Assert: проверки результата и вызовов моков
            result.Should().HaveCount(2);
            result.Should().Contain(x => x.Name == "Килограмм");
            result.Should().Contain(x => x.Name == "Грамм");

            unitRepoMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        // Тест сценария создания — когда DTO имеет пустой Id, должен быть вызван Create в репозитории.
        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateUnit()
        {
            // Arrange
            var dto = new UnitItemDto
            {
                Id = Guid.Empty,
                Name = "Литр"
            };

            var entity = new Unit
            {
                Name = dto.Name
            };

            _mapperMock.Setup(x => x.Map<Unit>(dto)).Returns(entity);

            var unitRepoMock = new Mock<IUnitRepository>();
            // Мокируем создание сущности в репозитории
            unitRepoMock.Setup(x => x.CreateASync(entity)).ReturnsAsync(entity);
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(unitRepoMock.Object);

            var service = CreateService();

            // Act
            await service.SaveAsync(dto);

            // Assert: проверяем, что маппер и репозиторий вызваны
            _mapperMock.Verify(x => x.Map<Unit>(dto), Times.Once);
            unitRepoMock.Verify(x => x.CreateASync(entity), Times.Once);
        }

        // Тест удаления — когда сущность найдена в репозитории, должен быть вызван Delete.
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
                Name = "Штука"
            };

            var unitRepoMock = new Mock<IUnitRepository>();
            // Мок поиска сущности по условию
            unitRepoMock.Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Unit, bool>>>()))
                        .ReturnsAsync(entity);
            unitRepoMock.Setup(x => x.DeleteAsync(entity)).Returns(Task.CompletedTask);
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(unitRepoMock.Object);

            var service = CreateService();

            // Act
            await service.DeleteAsync(dto);

            // Assert
            unitRepoMock.Verify(x => x.DeleteAsync(entity), Times.Once);
        }

        // Тест поиска с пустой строкой — должен возвращать все единицы (делегирует GetAllAsync).
        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAllUnits()
        {
            // Arrange
            var gram = new Unit { Name = "Грамм" };

            var unitRepoMock = new Mock<IUnitRepository>();
            unitRepoMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Unit> { gram });
            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(unitRepoMock.Object);

            _mapperMock.Setup(x => x.Map<UnitItemDto>(gram))
                       .Returns(new UnitItemDto { Id = gram.Id, Name = gram.Name });

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(" ");

            // Assert
            result.Should().HaveCount(1);
            result.Single().Name.Should().Be("Грамм");

            unitRepoMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        // Тест поиска с текстом — должен вызывать условный поиск в репозитории и корректно маппить результат.
        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldReturnFilteredUnits()
        {
            // Arrange
            var gram = new Unit { Name = "Грамм" };
            var kilogram = new Unit { Name = "Килограмм" };

            var unitRepoMock = new Mock<IUnitRepository>();
            unitRepoMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<Unit, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Unit> { gram, kilogram });

            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(unitRepoMock.Object);

            _mapperMock.Setup(x => x.Map<UnitItemDto>(gram))
                       .Returns(new UnitItemDto { Id = gram.Id, Name = gram.Name });

            _mapperMock.Setup(x => x.Map<UnitItemDto>(kilogram))
                       .Returns(new UnitItemDto { Id = kilogram.Id, Name = kilogram.Name });

            var service = CreateService();

            // Act
            var result = await service.SearchAsync("гр");

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Name).Should().ContainInOrder("Грамм", "Килограмм");

            unitRepoMock.Verify(x => x.ConditionToListAsync(It.IsAny<Expression<Func<Unit, bool>>>(), It.IsAny<bool>()), Times.Once);
        }

        // Тест валидации: передача null в DeleteAsync должна бросать ArgumentNullException с правильным ParamName.
        [Fact]
        public async Task DeleteAsyncNullModel_ShouldThrowArgumentNullException()
        {
            var service = CreateService();

            // Act
            var act = async () => await service.DeleteAsync(null);

            // Assert
            var ex = await act.Should().ThrowAsync<ArgumentNullException>();
            ex.Which.ParamName.Should().Be("model");
        }

        // Тест удаления несуществующей сущности — сервис должен бросить InvalidOperationException с понятным сообщением.
        [Fact]
        public async Task DeleteAsyncNotFoundUnit_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new UnitItemDto
            {
                Id = Guid.NewGuid(),
                Name = "Штука"
            };

            var unitRepoMock = new Mock<IUnitRepository>();
            unitRepoMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Unit, bool>>>()))
                .ReturnsAsync((Unit)null);

            _dbRepositoryMock.Setup(x => x.unitRepository).Returns(unitRepoMock.Object);

            var service = CreateService();

            // Act
            var act = async () => await service.DeleteAsync(dto);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Message.Should().Be("Единица измерения не найдена.");
        }

        private UnitAppService CreateService()
        {
            return new UnitAppService(
                _dbRepositoryMock.Object,
                _mapperMock.Object);
        }
    }
}