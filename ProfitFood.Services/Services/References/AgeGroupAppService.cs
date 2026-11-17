using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника возрастных групп детей.
    /// </summary>
    public class AgeGroupAppService : IAgeGroupAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<AgeGroupAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника возрастных групп.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public AgeGroupAppService(IDbRepository dbRepository, ILogger<AgeGroupAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все возрастные группы, отсортированные по возрасту «от».</summary>
        public async Task<IReadOnlyCollection<AgeGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.ageGroupRepository.ToListAsync();

            return entities
                .OrderBy(x => x.AgeFromMonths)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Ищет возрастные группы по части наименования.</summary>
        public async Task<IReadOnlyCollection<AgeGroupDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.ageGroupRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText));

            return entities
                .OrderBy(x => x.AgeFromMonths)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Сохраняет возрастную группу: создаёт новую или обновляет существующую.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при пустом наименовании или неверном диапазоне возраста.</exception>
        public async Task SaveAsync(AgeGroupDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование возрастной группы не может быть пустым.");
            }

            if (model.AgeToMonths < model.AgeFromMonths)
            {
                throw new InvalidOperationException("Возраст «до» не может быть меньше возраста «от».");
            }

            try
            {
                if (model.Id == Guid.Empty)
                {
                    var newEntity = new AgeGroup();
                    Apply(newEntity, model, name);
                    await _dbRepository.ageGroupRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.ageGroupRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Возрастная группа не найдена: возможно, её удалили в другом окне.");
                }

                Apply(existingEntity, model, name);
                await _dbRepository.ageGroupRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить возрастную группу «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет возрастную группу.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(AgeGroupDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.ageGroupRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Возрастная группа не найдена.");
                }

                await _dbRepository.ageGroupRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить возрастную группу с идентификатором {Id}", model.Id);
                throw;
            }
        }

        private static AgeGroupDto MapToDto(AgeGroup entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            AgeFromMonths = entity.AgeFromMonths,
            AgeToMonths = entity.AgeToMonths
        };

        private static void Apply(AgeGroup entity, AgeGroupDto model, string name)
        {
            entity.Name = name;
            entity.AgeFromMonths = model.AgeFromMonths;
            entity.AgeToMonths = model.AgeToMonths;
        }
    }
}
