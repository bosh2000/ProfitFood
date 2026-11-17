using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника сезонов меню.
    /// </summary>
    public class SeasonAppService : ISeasonAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<SeasonAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника сезонов.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public SeasonAppService(IDbRepository dbRepository, ILogger<SeasonAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все сезоны, отсортированные по начальному месяцу.</summary>
        public async Task<IReadOnlyCollection<SeasonDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.seasonRepository.ToListAsync();

            return entities
                .OrderBy(x => x.MonthFrom)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Ищет сезоны по части наименования.</summary>
        public async Task<IReadOnlyCollection<SeasonDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.seasonRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText));

            return entities
                .OrderBy(x => x.MonthFrom)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Сохраняет сезон: создаёт новый или обновляет существующий.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при пустом наименовании или неверном диапазоне месяцев.</exception>
        public async Task SaveAsync(SeasonDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование сезона не может быть пустым.");
            }

            if (model.MonthFrom is < 1 or > 12 || model.MonthTo is < 1 or > 12)
            {
                throw new InvalidOperationException("Месяцы сезона должны быть в диапазоне от 1 до 12.");
            }

            try
            {
                if (model.Id == Guid.Empty)
                {
                    var newEntity = new Season();
                    Apply(newEntity, model, name);
                    await _dbRepository.seasonRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.seasonRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Сезон не найден: возможно, его удалили в другом окне.");
                }

                Apply(existingEntity, model, name);
                await _dbRepository.seasonRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить сезон «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет сезон.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(SeasonDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.seasonRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Сезон не найден.");
                }

                await _dbRepository.seasonRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить сезон с идентификатором {Id}", model.Id);
                throw;
            }
        }

        private static SeasonDto MapToDto(Season entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            MonthFrom = entity.MonthFrom,
            MonthTo = entity.MonthTo
        };

        private static void Apply(Season entity, SeasonDto model, string name)
        {
            entity.Name = name;
            entity.MonthFrom = model.MonthFrom;
            entity.MonthTo = model.MonthTo;
        }
    }
}
