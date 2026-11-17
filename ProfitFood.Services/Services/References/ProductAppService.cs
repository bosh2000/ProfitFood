using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника продуктов питания.
    /// </summary>
    public class ProductAppService : IProductAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<ProductAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника продуктов.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public ProductAppService(IDbRepository dbRepository, ILogger<ProductAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все продукты с наименованиями категории и единиц измерения.</summary>
        public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.productRepository.ToListAsync();
            var lookups = await LoadLookupsAsync();

            return entities
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(entity => MapToDto(entity, lookups))
                .ToList();
        }

        /// <summary>Ищет продукты по краткому или полному наименованию и артикулу.</summary>
        public async Task<IReadOnlyCollection<ProductDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.productRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText)
                                           || x.FullName.ToLower().Contains(normalizedSearchText)
                                           || (x.Article != null && x.Article.ToLower().Contains(normalizedSearchText)));
            var lookups = await LoadLookupsAsync();

            return entities
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(entity => MapToDto(entity, lookups))
                .ToList();
        }

        /// <summary>Сохраняет продукт: создаёт новый или обновляет существующий.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при некорректных полях или отсутствии связанных записей.</exception>
        public async Task SaveAsync(ProductDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Краткое наименование продукта не может быть пустым.");
            }

            if (model.ProductCategoryId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрана категория продукта.");
            }

            if (model.BaseUnitId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрана базовая единица измерения.");
            }

            if (model.MinStock < 0)
            {
                throw new InvalidOperationException("Минимальный остаток не может быть отрицательным.");
            }

            try
            {
                var category = await _dbRepository.productCategoryRepository
                    .FirstOfDefaultAsync(x => x.Id == model.ProductCategoryId);

                if (category is null)
                {
                    throw new InvalidOperationException("Выбранная категория продукта не найдена.");
                }

                var baseUnit = await _dbRepository.unitRepository
                    .FirstOfDefaultAsync(x => x.Id == model.BaseUnitId);

                if (baseUnit is null)
                {
                    throw new InvalidOperationException("Выбранная базовая единица измерения не найдена.");
                }

                if (model.StorageUnitId.HasValue && model.StorageUnitId.Value != Guid.Empty)
                {
                    var storageUnit = await _dbRepository.unitRepository
                        .FirstOfDefaultAsync(x => x.Id == model.StorageUnitId.Value);

                    if (storageUnit is null)
                    {
                        throw new InvalidOperationException("Выбранная складская единица измерения не найдена.");
                    }
                }

                if (model.Id == Guid.Empty)
                {
                    var newEntity = new Product();
                    Apply(newEntity, model, name);
                    await _dbRepository.productRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.productRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Продукт не найден: возможно, его удалили в другом окне.");
                }

                Apply(existingEntity, model, name);
                await _dbRepository.productRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить продукт «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет продукт.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(ProductDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.productRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Продукт не найден.");
                }

                await _dbRepository.productRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить продукт с идентификатором {Id}", model.Id);
                throw;
            }
        }

        /// <summary>
        /// Загружает справочники категорий и единиц измерения, чтобы заполнить отображаемые названия
        /// без запросов к базе на каждую строку списка.
        /// </summary>
        private async Task<ProductLookups> LoadLookupsAsync()
        {
            var categories = await _dbRepository.productCategoryRepository.ToListAsync();
            var units = await _dbRepository.unitRepository.ToListAsync();

            return new ProductLookups(
                categories.ToDictionary(x => x.Id, x => x.Name),
                units.ToDictionary(x => x.Id, x => x.Name));
        }

        private static ProductDto MapToDto(Product entity, ProductLookups lookups) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            FullName = entity.FullName,
            Article = entity.Article,
            ProductCategoryId = entity.ProductCategoryId,
            ProductCategoryName = lookups.CategoryNames.TryGetValue(entity.ProductCategoryId, out var categoryName)
                ? categoryName
                : string.Empty,
            BaseUnitId = entity.BaseUnitId,
            BaseUnitName = lookups.UnitNames.TryGetValue(entity.BaseUnitId, out var baseUnitName)
                ? baseUnitName
                : string.Empty,
            StorageUnitId = entity.StorageUnitId,
            StorageUnitName = entity.StorageUnitId.HasValue
                              && lookups.UnitNames.TryGetValue(entity.StorageUnitId.Value, out var storageUnitName)
                ? storageUnitName
                : string.Empty,
            SortOrder = entity.SortOrder,
            IsPerishable = entity.IsPerishable,
            MinStock = entity.MinStock
        };

        private static void Apply(Product entity, ProductDto model, string name)
        {
            entity.Name = name;
            entity.FullName = string.IsNullOrWhiteSpace(model.FullName) ? name : model.FullName.Trim();
            entity.Article = string.IsNullOrWhiteSpace(model.Article) ? null : model.Article.Trim();
            entity.ProductCategoryId = model.ProductCategoryId;
            entity.BaseUnitId = model.BaseUnitId;
            entity.StorageUnitId = model.StorageUnitId == Guid.Empty ? null : model.StorageUnitId;
            entity.SortOrder = model.SortOrder;
            entity.IsPerishable = model.IsPerishable;
            entity.MinStock = model.MinStock;
        }

        /// <summary>Справочники, нужные для отображения строк списка продуктов.</summary>
        private sealed record ProductLookups(
            IReadOnlyDictionary<Guid, string> CategoryNames,
            IReadOnlyDictionary<Guid, string> UnitNames);
    }
}
