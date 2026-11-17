using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника типов приёмов пищи.
    /// </summary>
    public class MealTypeAppService : IMealTypeAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<MealTypeAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника типов приёмов пищи.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public MealTypeAppService(IDbRepository dbRepository, ILogger<MealTypeAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все типы приёмов пищи в порядке сортировки.</summary>
        public async Task<IReadOnlyCollection<MealTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.mealTypeRepository.ToListAsync();

            return entities
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Ищет типы приёмов пищи по наименованию или коду.</summary>
        public async Task<IReadOnlyCollection<MealTypeDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.mealTypeRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText)
                                           || x.Code.ToLower().Contains(normalizedSearchText));

            return entities
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Сохраняет тип приёма пищи: создаёт новый или обновляет существующий.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при пустом наименовании или коде.</exception>
        public async Task SaveAsync(MealTypeDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование приёма пищи не может быть пустым.");
            }

            var code = model.Code?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Код приёма пищи не может быть пустым.");
            }

            try
            {
                if (model.Id == Guid.Empty)
                {
                    var newEntity = new MealType();
                    Apply(newEntity, model, name, code);
                    await _dbRepository.mealTypeRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.mealTypeRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Приём пищи не найден: возможно, его удалили в другом окне.");
                }

                Apply(existingEntity, model, name, code);
                await _dbRepository.mealTypeRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить приём пищи «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет тип приёма пищи.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(MealTypeDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.mealTypeRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Приём пищи не найден.");
                }

                await _dbRepository.mealTypeRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить приём пищи с идентификатором {Id}", model.Id);
                throw;
            }
        }

        private static MealTypeDto MapToDto(MealType entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            SortOrder = entity.SortOrder
        };

        private static void Apply(MealType entity, MealTypeDto model, string name, string code)
        {
            entity.Name = name;
            entity.Code = code;
            entity.SortOrder = model.SortOrder;
        }
    }
}
