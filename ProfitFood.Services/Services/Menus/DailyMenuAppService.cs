using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.Menus;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Services.Menus
{
    /// <summary>
    /// Сервис раздела «Меню»: список ежедневных меню, их сохранение и позиции.
    /// </summary>
    public class DailyMenuAppService : IDailyMenuAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;
        private readonly ILogger<DailyMenuAppService> _logger;

        /// <summary>
        /// Инициализирует сервис ежедневных меню.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="contextFactory">
        /// Фабрика контекста: позиции меню читаются вместе со справочниками в одном контексте.
        /// </param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public DailyMenuAppService(
            IDbRepository dbRepository,
            IDbContextFactory<ProfitFoodDbContext> contextFactory,
            ILogger<DailyMenuAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает меню, начиная с самого свежего.</summary>
        public async Task<IReadOnlyCollection<DailyMenuListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var menus = await _dbRepository.dailyMenuRepository.ToListAsync();
            var seasonNames = await LoadSeasonNamesAsync();

            return menus
                .OrderByDescending(x => x.MenuDate)
                .Select(menu => MapToDto(menu, seasonNames))
                .ToList();
        }

        /// <summary>Ищет меню по примечанию.</summary>
        public async Task<IReadOnlyCollection<DailyMenuListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var menus = await _dbRepository.dailyMenuRepository
                .ConditionToListAsync(x => x.Notes != null && x.Notes.ToLower().Contains(normalizedSearchText));
            var seasonNames = await LoadSeasonNamesAsync();

            return menus
                .OrderByDescending(x => x.MenuDate)
                .Select(menu => MapToDto(menu, seasonNames))
                .ToList();
        }

        /// <summary>Возвращает позиции меню: приём пищи, блюдо, техкарту и выход порции.</summary>
        public async Task<IReadOnlyCollection<DailyMenuItemDto>> GetItemsAsync(Guid dailyMenuId, CancellationToken cancellationToken = default)
        {
            if (dailyMenuId == Guid.Empty)
            {
                return Array.Empty<DailyMenuItemDto>();
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var items = await context.DailyMenuItems
                .AsNoTracking()
                .Where(x => x.DailyMenuId == dailyMenuId)
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                return Array.Empty<DailyMenuItemDto>();
            }

            var mealTypes = await context.MealTypes.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
            var dishes = await context.Dishes.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
            var versions = await context.RecipeCardVersions.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
            var cards = await context.RecipeCards.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
            var units = await context.Units.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);

            return items
                .OrderBy(item => mealTypes.TryGetValue(item.MealTypeId, out var mealType) ? mealType.SortOrder : 0)
                .ThenBy(item => item.SortOrder)
                .Select(item =>
                {
                    versions.TryGetValue(item.RecipeCardVersionId, out var version);

                    var cardNumber = version is not null && cards.TryGetValue(version.RecipeCardId, out var card)
                        ? card.CardNumber
                        : string.Empty;

                    var outputUnitName = version is not null
                                         && units.TryGetValue(version.OutputUnitId, out var unit)
                        ? unit.ShortName
                        : string.Empty;

                    return new DailyMenuItemDto
                    {
                        Id = item.Id,
                        MealTypeId = item.MealTypeId,
                        DishId = item.DishId,
                        RecipeCardVersionId = item.RecipeCardVersionId,
                        SortOrder = item.SortOrder,
                        MealTypeName = mealTypes.TryGetValue(item.MealTypeId, out var mealTypeInfo)
                            ? mealTypeInfo.Name
                            : string.Empty,
                        DishName = dishes.TryGetValue(item.DishId, out var dish)
                            ? dish.Name
                            : string.Empty,
                        RecipeCardNumber = cardNumber,
                        OutputQuantity = version?.OutputQuantity,
                        OutputUnitName = outputUnitName
                    };
                })
                .ToList();
        }

        /// <summary>Сохраняет блюда меню: обновляет существующие, добавляет новые и убирает лишние.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если список строк пуст.</exception>
        /// <exception cref="InvalidOperationException">Возникает при незаполненных ссылках или отсутствии меню.</exception>
        public async Task SaveItemsAsync(
            Guid dailyMenuId,
            IReadOnlyCollection<DailyMenuItemDto> rows,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(rows);

            if (dailyMenuId == Guid.Empty)
            {
                throw new InvalidOperationException("Сначала сохраните меню: блюда привязываются к конкретной дате.");
            }

            foreach (var row in rows)
            {
                if (row.MealTypeId == Guid.Empty)
                {
                    throw new InvalidOperationException("В строке меню не выбран приём пищи.");
                }

                if (row.DishId == Guid.Empty)
                {
                    throw new InvalidOperationException("В строке меню не выбрано блюдо.");
                }

                if (row.RecipeCardVersionId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "У выбранного блюда нет технологической карты: сначала заполните техкарту.");
                }
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var stored = await context.DailyMenuItems
                    .Where(x => x.DailyMenuId == dailyMenuId)
                    .ToListAsync(cancellationToken);

                var incomingIds = rows
                    .Where(x => x.Id != Guid.Empty)
                    .Select(x => x.Id)
                    .ToHashSet();

                foreach (var removed in stored.Where(x => !incomingIds.Contains(x.Id)))
                {
                    context.DailyMenuItems.Remove(removed);
                }

                var sortOrder = 1;

                foreach (var row in rows)
                {
                    var target = row.Id == Guid.Empty
                        ? null
                        : stored.FirstOrDefault(x => x.Id == row.Id);

                    if (target is null)
                    {
                        target = new DailyMenuItem { DailyMenuId = dailyMenuId };
                        context.DailyMenuItems.Add(target);
                    }

                    target.MealTypeId = row.MealTypeId;
                    target.DishId = row.DishId;
                    target.RecipeCardVersionId = row.RecipeCardVersionId;
                    target.SortOrder = sortOrder++;
                }

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить блюда меню {DailyMenuId}", dailyMenuId);
                throw;
            }
        }

        /// <summary>Сохраняет меню: создаёт новое или обновляет существующее.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если выбранный сезон не найден или запись не существует.</exception>
        public async Task SaveAsync(DailyMenuListItemDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var seasonId = model.SeasonId == Guid.Empty ? null : model.SeasonId;

            if (seasonId.HasValue)
            {
                var season = await _dbRepository.seasonRepository
                    .FirstOfDefaultAsync(x => x.Id == seasonId.Value);

                if (season is null)
                {
                    throw new InvalidOperationException("Выбранный сезон не найден.");
                }
            }

            var notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();

            try
            {
                if (model.Id == Guid.Empty)
                {
                    // Новая запись создаётся черновиком: статус меняется позже, при работе с меню.
                    var newEntity = new DailyMenu
                    {
                        MenuDate = model.MenuDate,
                        SeasonId = seasonId,
                        Status = DailyMenuStatus.Draft,
                        Notes = notes,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _dbRepository.dailyMenuRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.dailyMenuRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Меню не найдено: возможно, его удалили в другом окне.");
                }

                existingEntity.MenuDate = model.MenuDate;
                existingEntity.SeasonId = seasonId;
                existingEntity.Notes = notes;
                existingEntity.UpdatedAt = DateTime.Now;

                await _dbRepository.dailyMenuRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить меню на {MenuDate}", model.MenuDate);
                throw;
            }
        }

        /// <summary>Удаляет меню на дату.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если меню не найдено.</exception>
        public async Task DeleteAsync(DailyMenuListItemDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.dailyMenuRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Меню не найдено: возможно, его удалили в другом окне.");
                }

                await _dbRepository.dailyMenuRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить меню на {MenuDate}", model.MenuDate);
                throw;
            }
        }

        /// <summary>
        /// Загружает словарь «идентификатор сезона — наименование» одним запросом.
        /// </summary>
        private async Task<IReadOnlyDictionary<Guid, string>> LoadSeasonNamesAsync()
        {
            var seasons = await _dbRepository.seasonRepository.ToListAsync();

            return seasons.ToDictionary(x => x.Id, x => x.Name);
        }

        private static DailyMenuListItemDto MapToDto(DailyMenu menu, IReadOnlyDictionary<Guid, string> seasonNames) => new()
        {
            Id = menu.Id,
            MenuDate = menu.MenuDate,
            SeasonId = menu.SeasonId,
            SeasonName = menu.SeasonId.HasValue && seasonNames.TryGetValue(menu.SeasonId.Value, out var seasonName)
                ? seasonName
                : string.Empty,
            StatusName = menu.Status.ToString(),
            Notes = menu.Notes,
            UpdatedAt = menu.UpdatedAt
        };
    }
}
