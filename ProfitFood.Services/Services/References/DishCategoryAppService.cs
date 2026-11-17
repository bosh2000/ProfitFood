using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника категорий блюд.
    /// </summary>
    public class DishCategoryAppService : IDishCategoryAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<DishCategoryAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника категорий блюд.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public DishCategoryAppService(IDbRepository dbRepository, ILogger<DishCategoryAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все категории блюд в порядке сортировки.</summary>
        public async Task<IReadOnlyCollection<DishCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.dishCategoryRepository.ToListAsync();

            return entities
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Ищет категории блюд по части наименования.</summary>
        public async Task<IReadOnlyCollection<DishCategoryDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.dishCategoryRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText));

            return entities
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>Сохраняет категорию блюда: создаёт новую или обновляет существующую.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при пустом наименовании.</exception>
        public async Task SaveAsync(DishCategoryDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование категории блюда не может быть пустым.");
            }

            try
            {
                if (model.Id == Guid.Empty)
                {
                    var newEntity = new DishCategory();
                    Apply(newEntity, model, name);
                    await _dbRepository.dishCategoryRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.dishCategoryRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Категория блюда не найдена: возможно, её удалили в другом окне.");
                }

                Apply(existingEntity, model, name);
                await _dbRepository.dishCategoryRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить категорию блюда «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет категорию блюда.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(DishCategoryDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.dishCategoryRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Категория блюда не найдена.");
                }

                await _dbRepository.dishCategoryRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить категорию блюда с идентификатором {Id}", model.Id);
                throw;
            }
        }

        private static DishCategoryDto MapToDto(DishCategory entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            SortOrder = entity.SortOrder
        };

        private static void Apply(DishCategory entity, DishCategoryDto model, string name)
        {
            entity.Name = name;
            entity.SortOrder = model.SortOrder;
        }
    }
}
