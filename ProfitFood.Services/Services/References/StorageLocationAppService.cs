using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника мест хранения продуктов.
    /// </summary>
    public class StorageLocationAppService : IStorageLocationAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<StorageLocationAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника складов.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public StorageLocationAppService(IDbRepository dbRepository, ILogger<StorageLocationAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все места хранения, отсортированные по наименованию.</summary>
        public async Task<IReadOnlyCollection<StorageLocationDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.storageLocationRepository.ToListAsync();

            return entities
                .OrderBy(x => x.Name)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Ищет места хранения по наименованию или коду.</summary>
        public async Task<IReadOnlyCollection<StorageLocationDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.storageLocationRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText)
                                           || x.Code.ToLower().Contains(normalizedSearchText));

            return entities
                .OrderBy(x => x.Name)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Сохраняет место хранения: создаёт новое или обновляет существующее.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при пустом наименовании или коде.</exception>
        public async Task SaveAsync(StorageLocationDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование склада не может быть пустым.");
            }

            var code = model.Code?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Код склада не может быть пустым.");
            }

            try
            {
                if (model.Id == Guid.Empty)
                {
                    var newEntity = new StorageLocation();
                    Apply(newEntity, model, name, code);
                    await _dbRepository.storageLocationRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.storageLocationRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Склад не найден: возможно, его удалили в другом окне.");
                }

                Apply(existingEntity, model, name, code);
                await _dbRepository.storageLocationRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить склад «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет место хранения.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(StorageLocationDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.storageLocationRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Склад не найден.");
                }

                await _dbRepository.storageLocationRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить склад с идентификатором {Id}", model.Id);
                throw;
            }
        }

        private static StorageLocationDto MapToDto(StorageLocation entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code
        };

        private static void Apply(StorageLocation entity, StorageLocationDto model, string name, string code)
        {
            entity.Name = name;
            entity.Code = code;
        }
    }
}
