using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProfitFood.Applications.Dto.Menus;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.Menus;
using Microsoft.EntityFrameworkCore;
using ProfitFoot.Infrastructure;
using System.Linq.Expressions;
using Xunit;

namespace ProfitFood.Tests.Applications.Services.Menus
{
    public class DailyMenuAppServiceTests
    {
        private readonly Mock<IDbRepository> _dbRepositoryMock;
        private readonly Mock<IDailyMenuRepository> _dailyMenuRepositoryMock;
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly Mock<IDbContextFactory<ProfitFoodDbContext>> _contextFactoryMock;

        public DailyMenuAppServiceTests()
        {
            _dbRepositoryMock = new Mock<IDbRepository>();
            _dailyMenuRepositoryMock = new Mock<IDailyMenuRepository>();
            _seasonRepositoryMock = new Mock<ISeasonRepository>();

            _dbRepositoryMock.Setup(x => x.dailyMenuRepository).Returns(_dailyMenuRepositoryMock.Object);
            _dbRepositoryMock.Setup(x => x.seasonRepository).Returns(_seasonRepositoryMock.Object);
            _contextFactoryMock = new Mock<IDbContextFactory<ProfitFoodDbContext>>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldFillSeasonNameAndStatus()
        {
            var season = new Season { Name = "Зима", MonthFrom = 12, MonthTo = 2 };
            var menu = new DailyMenu
            {
                MenuDate = new DateOnly(2026, 1, 15),
                SeasonId = season.Id,
                Status = DailyMenuStatus.Approved,
                Notes = "Утверждено заведующей"
            };

            _seasonRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Season> { season });
            _dailyMenuRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<DailyMenu> { menu });

            var service = CreateService();

            var result = await service.GetAllAsync();

            var item = result.Should().ContainSingle().Subject;
            item.SeasonName.Should().Be("Зима");
            item.StatusName.Should().Be(nameof(DailyMenuStatus.Approved));
            item.MenuDate.Should().Be(new DateOnly(2026, 1, 15));
            item.Notes.Should().Be("Утверждено заведующей");
        }

        [Fact]
        public async Task GetAllAsync_WhenSeasonIsUnknown_ShouldReturnEmptySeasonName()
        {
            var menu = new DailyMenu
            {
                MenuDate = new DateOnly(2026, 2, 1),
                SeasonId = Guid.NewGuid(),
                Status = DailyMenuStatus.Draft
            };

            _seasonRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Season>());
            _dailyMenuRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<DailyMenu> { menu });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().SeasonName.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WhenSeasonIsNotSet_ShouldReturnEmptySeasonName()
        {
            var menu = new DailyMenu { MenuDate = new DateOnly(2026, 3, 1), SeasonId = null };

            _seasonRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Season>());
            _dailyMenuRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<DailyMenu> { menu });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Single().SeasonName.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByMenuDateDescending()
        {
            _seasonRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Season>());
            _dailyMenuRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<DailyMenu>
                {
                    new() { MenuDate = new DateOnly(2026, 3, 10) },
                    new() { MenuDate = new DateOnly(2026, 3, 12) },
                    new() { MenuDate = new DateOnly(2026, 3, 11) }
                });

            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Select(x => x.MenuDate).Should().ContainInOrder(
                new DateOnly(2026, 3, 12),
                new DateOnly(2026, 3, 11),
                new DateOnly(2026, 3, 10));
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextIsEmpty_ShouldReturnAll()
        {
            _seasonRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Season>());
            _dailyMenuRepositoryMock
                .Setup(x => x.ToListAsync())
                .ReturnsAsync(new List<DailyMenu> { new() { MenuDate = new DateOnly(2026, 3, 12) } });

            var service = CreateService();

            var result = await service.SearchAsync("  ");

            result.Should().HaveCount(1);
            _dailyMenuRepositoryMock.Verify(x => x.ToListAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchTextProvided_ShouldUseConditionQuery()
        {
            _seasonRepositoryMock.Setup(x => x.ToListAsync()).ReturnsAsync(new List<Season>());
            _dailyMenuRepositoryMock
                .Setup(x => x.ConditionToListAsync(It.IsAny<Expression<Func<DailyMenu, bool>>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<DailyMenu> { new() { MenuDate = new DateOnly(2026, 3, 12), Notes = "праздничное" } });

            var service = CreateService();

            var result = await service.SearchAsync("празд");

            result.Should().HaveCount(1);
            _dailyMenuRepositoryMock.Verify(
                x => x.ConditionToListAsync(It.IsAny<Expression<Func<DailyMenu, bool>>>(), It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityExists_ShouldDelete()
        {
            var menu = new DailyMenu { MenuDate = new DateOnly(2026, 3, 12) };

            _dailyMenuRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DailyMenu, bool>>>()))
                .ReturnsAsync(menu);
            _dailyMenuRepositoryMock
                .Setup(x => x.DeleteAsync(It.IsAny<DailyMenu>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.DeleteAsync(new DailyMenuListItemDto { Id = menu.Id, MenuDate = menu.MenuDate });

            _dailyMenuRepositoryMock.Verify(x => x.DeleteAsync(menu), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _dailyMenuRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DailyMenu, bool>>>()))
                .ReturnsAsync((DailyMenu)null!);

            var service = CreateService();

            var act = async () => await service.DeleteAsync(
                new DailyMenuListItemDto { Id = Guid.NewGuid(), MenuDate = new DateOnly(2026, 3, 12) });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenIdEmpty_ShouldCreateDraftMenu()
        {
            DailyMenu? created = null;
            _dailyMenuRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<DailyMenu>()))
                .Callback<DailyMenu>(entity => created = entity)
                .ReturnsAsync((DailyMenu entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new DailyMenuListItemDto
            {
                MenuDate = new DateOnly(2026, 4, 1),
                Notes = "  Праздничное меню  "
            });

            created.Should().NotBeNull();
            created!.MenuDate.Should().Be(new DateOnly(2026, 4, 1));
            created.Notes.Should().Be("Праздничное меню");
            created.Status.Should().Be(DailyMenuStatus.Draft);
            created.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task SaveAsync_WhenSeasonIsSelected_ShouldSetSeasonId()
        {
            var season = new Season { Name = "Весна", MonthFrom = 3, MonthTo = 5 };

            _seasonRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Season, bool>>>()))
                .ReturnsAsync(season);

            DailyMenu? created = null;
            _dailyMenuRepositoryMock
                .Setup(x => x.CreateASync(It.IsAny<DailyMenu>()))
                .Callback<DailyMenu>(entity => created = entity)
                .ReturnsAsync((DailyMenu entity) => entity);

            var service = CreateService();

            await service.SaveAsync(new DailyMenuListItemDto
            {
                MenuDate = new DateOnly(2026, 4, 1),
                SeasonId = season.Id
            });

            created.Should().NotBeNull();
            created!.SeasonId.Should().Be(season.Id);
        }

        [Fact]
        public async Task SaveAsync_WhenSeasonDoesNotExist_ShouldThrowInvalidOperationException()
        {
            _seasonRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<Season, bool>>>()))
                .ReturnsAsync((Season)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new DailyMenuListItemDto
            {
                MenuDate = new DateOnly(2026, 4, 1),
                SeasonId = Guid.NewGuid()
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveAsync_WhenEntityExists_ShouldUpdate()
        {
            var menu = new DailyMenu
            {
                MenuDate = new DateOnly(2026, 3, 12),
                Notes = "Старое",
                UpdatedAt = DateTime.Now.AddDays(-1)
            };

            _dailyMenuRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DailyMenu, bool>>>()))
                .ReturnsAsync(menu);
            _dailyMenuRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<DailyMenu>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.SaveAsync(new DailyMenuListItemDto
            {
                Id = menu.Id,
                MenuDate = new DateOnly(2026, 4, 1),
                Notes = " Новое "
            });

            menu.MenuDate.Should().Be(new DateOnly(2026, 4, 1));
            menu.Notes.Should().Be("Новое");
            _dailyMenuRepositoryMock.Verify(x => x.UpdateAsync(menu), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WhenEntityNotFound_ShouldThrowInvalidOperationException()
        {
            _dailyMenuRepositoryMock
                .Setup(x => x.FirstOfDefaultAsync(It.IsAny<Expression<Func<DailyMenu, bool>>>()))
                .ReturnsAsync((DailyMenu)null!);

            var service = CreateService();

            var act = async () => await service.SaveAsync(new DailyMenuListItemDto
            {
                Id = Guid.NewGuid(),
                MenuDate = new DateOnly(2026, 4, 1)
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private IDailyMenuAppService CreateService()
            => new DailyMenuAppService(
                _dbRepositoryMock.Object,
                _contextFactoryMock.Object,
                NullLogger<DailyMenuAppService>.Instance);
    }
}
