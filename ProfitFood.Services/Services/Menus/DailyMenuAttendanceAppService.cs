using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.Menus;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Menus;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Services.Menus
{
    /// <summary>
    /// Сервис посещаемости: план и факт по группам детей на дату меню.
    /// </summary>
    public class DailyMenuAttendanceAppService : IDailyMenuAttendanceAppService
    {
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;
        private readonly IChildGroupAppService _childGroupService;
        private readonly ILogger<DailyMenuAttendanceAppService> _logger;

        /// <summary>
        /// Инициализирует сервис посещаемости.
        /// </summary>
        /// <param name="contextFactory">Фабрика контекста для чтения и записи посещаемости.</param>
        /// <param name="childGroupService">Сервис групп детей: плановая численность по умолчанию.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public DailyMenuAttendanceAppService(
            IDbContextFactory<ProfitFoodDbContext> contextFactory,
            IChildGroupAppService childGroupService,
            ILogger<DailyMenuAttendanceAppService> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _childGroupService = childGroupService ?? throw new ArgumentNullException(nameof(childGroupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает посещаемость по группам детей для меню на дату.</summary>
        public async Task<IReadOnlyCollection<DailyMenuAttendanceDto>> GetByMenuAsync(Guid dailyMenuId, CancellationToken cancellationToken = default)
        {
            var groups = await _childGroupService.GetAllAsync(cancellationToken);

            if (groups.Count == 0)
            {
                return Array.Empty<DailyMenuAttendanceDto>();
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var stored = dailyMenuId == Guid.Empty
                ? new List<DailyMenuAttendance>()
                : await context.DailyMenuAttendances
                    .AsNoTracking()
                    .Where(x => x.DailyMenuId == dailyMenuId)
                    .ToListAsync(cancellationToken);

            var storedByGroup = stored.ToDictionary(x => x.ChildGroupId);

            return groups
                .Select(group =>
                {
                    storedByGroup.TryGetValue(group.Id, out var saved);

                    return new DailyMenuAttendanceDto
                    {
                        ChildGroupId = group.Id,
                        ChildGroupName = group.Name,
                        // Если по группе ещё ничего не сохраняли, планом считается численность из справочника.
                        PlannedCount = saved?.PlannedCount ?? group.PlannedChildrenCount,
                        ActualCount = saved?.ActualCount ?? 0
                    };
                })
                .OrderBy(x => x.ChildGroupName)
                .ToList();
        }

        /// <summary>Сохраняет посещаемость по группам детей для меню на дату.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если список строк пуст.</exception>
        /// <exception cref="InvalidOperationException">Возникает при некорректных количествах или отсутствии меню.</exception>
        public async Task SaveAsync(Guid dailyMenuId, IReadOnlyCollection<DailyMenuAttendanceDto> rows, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(rows);

            if (dailyMenuId == Guid.Empty)
            {
                throw new InvalidOperationException("Сначала сохраните меню: посещаемость привязывается к конкретной дате.");
            }

            foreach (var row in rows)
            {
                if (row.PlannedCount < 0)
                {
                    throw new InvalidOperationException(
                        $"Плановое количество для группы «{row.ChildGroupName}» не может быть отрицательным.");
                }

                if (row.ActualCount < 0 || row.ActualCount > row.PlannedCount)
                {
                    throw new InvalidOperationException(
                        $"Фактическое количество для группы «{row.ChildGroupName}» должно быть от 0 до {row.PlannedCount}.");
                }
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var stored = await context.DailyMenuAttendances
                    .Where(x => x.DailyMenuId == dailyMenuId)
                    .ToListAsync(cancellationToken);

                var storedByGroup = stored.ToDictionary(x => x.ChildGroupId);

                foreach (var row in rows)
                {
                    if (storedByGroup.TryGetValue(row.ChildGroupId, out var entity))
                    {
                        entity.PlannedCount = row.PlannedCount;
                        entity.ActualCount = row.ActualCount;
                        continue;
                    }

                    context.DailyMenuAttendances.Add(new DailyMenuAttendance
                    {
                        DailyMenuId = dailyMenuId,
                        ChildGroupId = row.ChildGroupId,
                        PlannedCount = row.PlannedCount,
                        ActualCount = row.ActualCount
                    });
                }

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить посещаемость для меню {DailyMenuId}", dailyMenuId);
                throw;
            }
        }
    }
}
