using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.Menus;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.Menus
{
    /// <summary>
    /// Сервис раздела «Меню»: список ежедневных меню и их сохранение.
    /// </summary>
    public class DailyMenuAppService : IDailyMenuAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<DailyMenuAppService> _logger;

        /// <summary>
        /// Инициализирует сервис ежедневных меню.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public DailyMenuAppService(IDbRepository dbRepository, ILogger<DailyMenuAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
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
