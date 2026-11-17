using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника блюд.
    /// </summary>
    public class DishAppService : IDishAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<DishAppService> _logger;

        /// <summary>
        /// Инициализирует сервис блюд.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public DishAppService(IDbRepository dbRepository, ILogger<DishAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает блюда, отсортированные по наименованию.</summary>
        public async Task<IReadOnlyCollection<DishDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var dishes = await _dbRepository.dishRepository.ToListAsync();
            var lookups = await LoadLookupsAsync();

            return dishes
                .OrderBy(x => x.Name)
                .Select(dish => MapToDto(dish, lookups))
                .ToList();
        }

        /// <summary>Ищет блюда по наименованию или коду.</summary>
        public async Task<IReadOnlyCollection<DishDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var dishes = await _dbRepository.dishRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText)
                                           || x.Code.ToLower().Contains(normalizedSearchText));
            var lookups = await LoadLookupsAsync();

            return dishes
                .OrderBy(x => x.Name)
                .Select(dish => MapToDto(dish, lookups))
                .ToList();
        }

        /// <summary>Сохраняет блюдо: создаёт новое или обновляет существующее.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при некорректных данных или отсутствии связанных записей.</exception>
        public async Task SaveAsync(DishDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            var code = model.Code?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование блюда не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Код блюда не может быть пустым.");
            }

            if (model.DishCategoryId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрана категория блюда.");
            }

            if (model.MealTypeId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбран приём пищи.");
            }

            try
            {
                var category = await _dbRepository.dishCategoryRepository
                    .FirstOfDefaultAsync(x => x.Id == model.DishCategoryId);

                if (category is null)
                {
                    throw new InvalidOperationException("Выбранная категория блюда не найдена.");
                }

                var mealType = await _dbRepository.mealTypeRepository
                    .FirstOfDefaultAsync(x => x.Id == model.MealTypeId);

                if (mealType is null)
                {
                    throw new InvalidOperationException("Выбранный приём пищи не найден.");
                }

                var notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();

                if (model.Id == Guid.Empty)
                {
                    var newEntity = new Dish
                    {
                        Name = name,
                        Code = code,
                        DishCategoryId = model.DishCategoryId,
                        MealTypeId = model.MealTypeId,
                        Notes = notes
                    };

                    await _dbRepository.dishRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.dishRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Блюдо не найдено: возможно, его удалили в другом окне.");
                }

                existingEntity.Name = name;
                existingEntity.Code = code;
                existingEntity.DishCategoryId = model.DishCategoryId;
                existingEntity.MealTypeId = model.MealTypeId;
                existingEntity.Notes = notes;

                await _dbRepository.dishRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить блюдо «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет блюдо.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если блюдо не найдено.</exception>
        public async Task DeleteAsync(DishDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.dishRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Блюдо не найдено: возможно, его удалили в другом окне.");
                }

                await _dbRepository.dishRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить блюдо с идентификатором {Id}", model.Id);
                throw;
            }
        }

        /// <summary>
        /// Загружает словари категорий блюд и приёмов пищи одним запросом каждый.
        /// </summary>
        private async Task<DishLookups> LoadLookupsAsync()
        {
            var categories = await _dbRepository.dishCategoryRepository.ToListAsync();
            var mealTypes = await _dbRepository.mealTypeRepository.ToListAsync();

            return new DishLookups(
                categories.ToDictionary(x => x.Id, x => x.Name),
                mealTypes.ToDictionary(x => x.Id, x => x.Name));
        }

        private static DishDto MapToDto(Dish dish, DishLookups lookups) => new()
        {
            Id = dish.Id,
            Name = dish.Name,
            Code = dish.Code,
            DishCategoryId = dish.DishCategoryId,
            DishCategoryName = lookups.CategoryNames.TryGetValue(dish.DishCategoryId, out var categoryName)
                ? categoryName
                : string.Empty,
            MealTypeId = dish.MealTypeId,
            MealTypeName = lookups.MealTypeNames.TryGetValue(dish.MealTypeId, out var mealTypeName)
                ? mealTypeName
                : string.Empty,
            Notes = dish.Notes
        };

        /// <summary>Справочники, нужные для отображения блюд.</summary>
        private sealed record DishLookups(
            IReadOnlyDictionary<Guid, string> CategoryNames,
            IReadOnlyDictionary<Guid, string> MealTypeNames);
    }
}
