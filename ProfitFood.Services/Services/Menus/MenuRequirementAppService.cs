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
    /// Сервис меню-требования: расчёт потребности в продуктах по меню на дату.
    /// </summary>
    public class MenuRequirementAppService : IMenuRequirementAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;
        private readonly ILogger<MenuRequirementAppService> _logger;

        /// <summary>
        /// Инициализирует сервис меню-требований.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям для журнала.</param>
        /// <param name="contextFactory">Фабрика контекста: расчёт и сохранение идут в одной транзакции.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public MenuRequirementAppService(
            IDbRepository dbRepository,
            IDbContextFactory<ProfitFoodDbContext> contextFactory,
            ILogger<MenuRequirementAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает журнал меню-требований, начиная с самого свежего.</summary>
        public async Task<IReadOnlyCollection<MenuRequirementDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var requirements = await _dbRepository.menuRequirementRepository.ToListAsync();

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var locations = await context.StorageLocations.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return requirements
                .OrderByDescending(x => x.RequirementDate)
                .ThenByDescending(x => x.Number)
                .Select(requirement => new MenuRequirementDto
                {
                    Id = requirement.Id,
                    Number = requirement.Number,
                    RequirementDate = requirement.RequirementDate,
                    DailyMenuId = requirement.DailyMenuId,
                    StorageLocationId = requirement.StorageLocationId,
                    StorageLocationName = locations.TryGetValue(requirement.StorageLocationId, out var name) ? name : string.Empty,
                    Status = requirement.Status,
                    TotalChildrenCount = requirement.TotalChildrenCount,
                    Notes = requirement.Notes
                })
                .ToList();
        }

        /// <summary>Возвращает документ вместе со строками.</summary>
        public async Task<MenuRequirementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var requirement = await context.MenuRequirements
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (requirement is null)
            {
                return null;
            }

            var locations = await context.StorageLocations.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var products = await context.Products.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var units = await context.Units.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.ShortName, cancellationToken);

            return new MenuRequirementDto
            {
                Id = requirement.Id,
                Number = requirement.Number,
                RequirementDate = requirement.RequirementDate,
                DailyMenuId = requirement.DailyMenuId,
                StorageLocationId = requirement.StorageLocationId,
                StorageLocationName = locations.TryGetValue(requirement.StorageLocationId, out var locationName)
                    ? locationName
                    : string.Empty,
                Status = requirement.Status,
                TotalChildrenCount = requirement.TotalChildrenCount,
                Notes = requirement.Notes,
                Items = requirement.Items
                    .OrderBy(x => x.SortOrder)
                    .Select(item => new MenuRequirementItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = products.TryGetValue(item.ProductId, out var productName) ? productName : string.Empty,
                        UnitId = item.UnitId,
                        UnitName = units.TryGetValue(item.UnitId, out var unitName) ? unitName : string.Empty,
                        Quantity = item.Quantity,
                        SortOrder = item.SortOrder
                    })
                    .ToList()
            };
        }

        /// <summary>Рассчитывает меню-требование по меню на дату.</summary>
        /// <exception cref="InvalidOperationException">Возникает, если меню пусто, нет ингредиентов, нет детей или не заполнены склады.</exception>
        public async Task<Guid> CalculateAsync(Guid dailyMenuId, CancellationToken cancellationToken = default)
        {
            if (dailyMenuId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрано меню для расчёта.");
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var menu = await context.DailyMenus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dailyMenuId, cancellationToken)
                    ?? throw new InvalidOperationException("Меню не найдено: возможно, его удалили в другом окне.");

                var positions = await context.DailyMenuItems
                    .AsNoTracking()
                    .Where(x => x.DailyMenuId == dailyMenuId)
                    .ToListAsync(cancellationToken);

                if (positions.Count == 0)
                {
                    throw new InvalidOperationException("В меню нет ни одной позиции: рассчитывать нечего.");
                }

                var versionIds = positions.Select(x => x.RecipeCardVersionId).Distinct().ToList();

                var versions = await context.RecipeCardVersions
                    .AsNoTracking()
                    .Where(x => versionIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                var ingredients = await context.RecipeIngredients
                    .AsNoTracking()
                    .Where(x => versionIds.Contains(x.RecipeCardVersionId))
                    .ToListAsync(cancellationToken);

                var products = await context.Products.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
                var units = await context.Units.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);

                var portionGroups = await ResolveChildrenGroupsAsync(context, dailyMenuId, cancellationToken);
                var childrenCount = portionGroups.Sum(x => x.Count);

                var outputs = await context.DishOutputs
                    .AsNoTracking()
                    .Where(x => versionIds.Contains(x.RecipeCardVersionId))
                    .ToListAsync(cancellationToken);

                // Потребность агрегируется по продукту: одно и то же молоко может встречаться в нескольких блюдах.
                var quantities = new Dictionary<Guid, decimal>();

                foreach (var position in positions)
                {
                    var version = versions.FirstOrDefault(x => x.Id == position.RecipeCardVersionId);

                    if (version is null)
                    {
                        continue;
                    }

                    var versionIngredients = ingredients
                        .Where(x => x.RecipeCardVersionId == version.Id)
                        .ToList();

                    foreach (var group in portionGroups)
                    {
                        // Норма порции зависит от возраста: коэффициент — отношение выхода группы к выходу версии.
                        var output = outputs.FirstOrDefault(
                            x => x.RecipeCardVersionId == version.Id && x.AgeGroupId == group.AgeGroupId);

                        var coefficient = output is not null && version.OutputQuantity > 0m
                            ? output.OutputQuantity / version.OutputQuantity
                            : 1m;

                        foreach (var ingredient in versionIngredients)
                        {
                            if (!products.TryGetValue(ingredient.ProductId, out var product)
                                || !units.TryGetValue(ingredient.UnitId, out var ingredientUnit)
                                || !units.TryGetValue(product.BaseUnitId, out var baseUnit))
                            {
                                continue;
                            }

                            var factor = baseUnit.BaseFactor == 0m ? 1m : ingredientUnit.BaseFactor / baseUnit.BaseFactor;
                            var quantity = group.Count * ingredient.NetQuantity * coefficient * factor;

                            quantities[product.Id] = quantities.TryGetValue(product.Id, out var current)
                                ? current + quantity
                                : quantity;
                        }
                    }
                }

                if (quantities.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Не удалось рассчитать количество: у блюд меню не заполнены ингредиенты.");
                }

                var storage = await context.StorageLocations
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException("Не заполнен справочник складов: некуда выписать требование.");

                var entity = new MenuRequirement
                {
                    Number = await GenerateNumberAsync(context, menu.MenuDate, cancellationToken),
                    RequirementDate = menu.MenuDate,
                    DailyMenuId = dailyMenuId,
                    StorageLocationId = storage.Id,
                    Status = MenuRequirementStatus.Draft,
                    TotalChildrenCount = childrenCount,
                    CreatedAt = DateTime.Now
                };

                var sortOrder = 1;

                foreach (var pair in quantities.OrderBy(x => products[x.Key].Name))
                {
                    var product = products[pair.Key];

                    entity.Items.Add(new MenuRequirementItem
                    {
                        MenuRequirementId = entity.Id,
                        ProductId = product.Id,
                        UnitId = product.BaseUnitId,
                        Quantity = Math.Round(pair.Value, 3),
                        // Цена и сумма в этом документе не ведутся: важен только объём передачи на кухню.
                        Price = 0m,
                        Amount = 0m,
                        SortOrder = sortOrder++
                    });
                }

                context.MenuRequirements.Add(entity);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось рассчитать меню-требование для меню {DailyMenuId}", dailyMenuId);
                throw;
            }
        }

        /// <summary>Сохраняет правки черновика.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если документ не найден или уже не черновик.</exception>
        public async Task SaveAsync(MenuRequirementDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var entity = await context.MenuRequirements
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Меню-требование не найдено: возможно, его удалили в другом окне.");

                if (entity.Status != MenuRequirementStatus.Draft)
                {
                    throw new InvalidOperationException("Изменять можно только черновик требования.");
                }

                entity.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();

                context.MenuRequirementItems.RemoveRange(entity.Items);
                entity.Items.Clear();

                var sortOrder = 1;

                foreach (var item in model.Items)
                {
                    entity.Items.Add(new MenuRequirementItem
                    {
                        MenuRequirementId = entity.Id,
                        ProductId = item.ProductId,
                        UnitId = item.UnitId,
                        Quantity = item.Quantity,
                        Price = 0m,
                        Amount = 0m,
                        SortOrder = sortOrder++
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить меню-требование {Number}", model.Number);
                throw;
            }
        }

        /// <summary>Удаляет черновик требования.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если документ не найден или уже не черновик.</exception>
        public async Task DeleteAsync(MenuRequirementDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            try
            {
                var entity = await context.MenuRequirements
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Меню-требование не найдено: возможно, его удалили в другом окне.");

                if (entity.Status != MenuRequirementStatus.Draft)
                {
                    throw new InvalidOperationException("Удалить можно только черновик требования.");
                }

                context.MenuRequirements.Remove(entity);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить меню-требование {Number}", model.Number);
                throw;
            }
        }

        /// <summary>
        /// Определяет количество детей по группам: сначала факт посещаемости, затем план, затем плановая численность.
        /// </summary>
        private static async Task<IReadOnlyList<ChildGroupCount>> ResolveChildrenGroupsAsync(
            ProfitFoodDbContext context,
            Guid dailyMenuId,
            CancellationToken cancellationToken)
        {
            var childGroups = await context.ChildGroups
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var attendance = await context.DailyMenuAttendances
                .AsNoTracking()
                .Where(x => x.DailyMenuId == dailyMenuId)
                .ToListAsync(cancellationToken);

            if (attendance.Count > 0)
            {
                var fromAttendance = new List<ChildGroupCount>();

                foreach (var row in attendance)
                {
                    var count = row.ActualCount > 0 ? row.ActualCount : row.PlannedCount;

                    if (count <= 0)
                    {
                        continue;
                    }

                    var ageGroupId = childGroups.FirstOrDefault(x => x.Id == row.ChildGroupId)?.AgeGroupId ?? Guid.Empty;
                    fromAttendance.Add(new ChildGroupCount(ageGroupId, count));
                }

                if (fromAttendance.Count == 0)
                {
                    throw new InvalidOperationException("Не заполнена посещаемость: количество детей равно нулю.");
                }

                return fromAttendance;
            }

            var planned = childGroups
                .Where(x => x.PlannedChildrenCount > 0)
                .Select(x => new ChildGroupCount(x.AgeGroupId, x.PlannedChildrenCount))
                .ToList();

            if (planned.Count == 0)
            {
                throw new InvalidOperationException("Не заполнена посещаемость: количество детей равно нулю.");
            }

            return planned;
        }

        /// <summary>Количество детей в группе с указанием её возрастной группы.</summary>
        private sealed record ChildGroupCount(Guid AgeGroupId, int Count);

        /// <summary>Строит номер требования: МТ, год и порядковый номер внутри года.</summary>
        private static async Task<string> GenerateNumberAsync(
            ProfitFoodDbContext context,
            DateOnly requirementDate,
            CancellationToken cancellationToken)
        {
            var dates = await context.MenuRequirements
                .AsNoTracking()
                .Select(x => x.RequirementDate)
                .ToListAsync(cancellationToken);

            var numberInYear = dates.Count(x => x.Year == requirementDate.Year) + 1;

            return $"МТ-{requirementDate.Year}-{numberInYear:0000}";
        }
    }
}
