using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.Stocks;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.Stocks
{
    /// <summary>
    /// Сервис раздела «Склад»: остатки продуктов и их сохранение.
    /// </summary>
    public class StockAppService : IStockAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<StockAppService> _logger;

        /// <summary>
        /// Инициализирует сервис складских остатков.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public StockAppService(IDbRepository dbRepository, ILogger<StockAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает остатки по всем складам, отсортированные по наименованию продукта.</summary>
        public async Task<IReadOnlyCollection<StockBalanceDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var balances = await _dbRepository.stockBalanceRepository.ToListAsync();
            var lookups = await LoadLookupsAsync();

            return balances
                .Select(balance => MapToDto(balance, lookups))
                .OrderBy(x => x.ProductName)
                .ThenBy(x => x.StorageLocationName)
                .ToList();
        }

        /// <summary>Ищет остатки по наименованию продукта или склада.</summary>
        public async Task<IReadOnlyCollection<StockBalanceDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();
            var all = await GetAllAsync(cancellationToken);

            // Фильтр по отображаемым именам выполняется в памяти: связь продукта и склада
            // уже разрешена словарями, отдельный запрос к базе здесь не нужен.
            return all
                .Where(x => x.ProductName.ToLower().Contains(normalizedSearchText)
                            || x.StorageLocationName.ToLower().Contains(normalizedSearchText))
                .ToList();
        }

        /// <summary>Сохраняет остаток: создаёт новую запись или обновляет существующую.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при некорректных полях или отсутствии связанных записей.</exception>
        public async Task SaveAsync(StockBalanceDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (model.ProductId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбран продукт.");
            }

            if (model.StorageLocationId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбран склад.");
            }

            if (model.UnitId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрана единица измерения.");
            }

            if (model.Quantity < 0)
            {
                throw new InvalidOperationException("Количество не может быть отрицательным.");
            }

            try
            {
                var product = await _dbRepository.productRepository
                    .FirstOfDefaultAsync(x => x.Id == model.ProductId);

                if (product is null)
                {
                    throw new InvalidOperationException("Выбранный продукт не найден.");
                }

                var location = await _dbRepository.storageLocationRepository
                    .FirstOfDefaultAsync(x => x.Id == model.StorageLocationId);

                if (location is null)
                {
                    throw new InvalidOperationException("Выбранный склад не найден.");
                }

                var unit = await _dbRepository.unitRepository
                    .FirstOfDefaultAsync(x => x.Id == model.UnitId);

                if (unit is null)
                {
                    throw new InvalidOperationException("Выбранная единица измерения не найдена.");
                }

                if (model.Id == Guid.Empty)
                {
                    var newEntity = new StockBalance
                    {
                        ProductId = model.ProductId,
                        StorageLocationId = model.StorageLocationId,
                        UnitId = model.UnitId,
                        Quantity = model.Quantity,
                        UpdatedAt = DateTime.Now
                    };

                    await _dbRepository.stockBalanceRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.stockBalanceRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Остаток не найден: возможно, его удалили в другом окне.");
                }

                existingEntity.ProductId = model.ProductId;
                existingEntity.StorageLocationId = model.StorageLocationId;
                existingEntity.UnitId = model.UnitId;
                existingEntity.Quantity = model.Quantity;
                existingEntity.UpdatedAt = DateTime.Now;

                await _dbRepository.stockBalanceRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить остаток продукта {ProductId} на складе {StorageLocationId}",
                    model.ProductId, model.StorageLocationId);
                throw;
            }
        }

        /// <summary>
        /// Загружает справочники продуктов, складов и единиц одним запросом каждый.
        /// </summary>
        private async Task<StockLookups> LoadLookupsAsync()
        {
            var products = await _dbRepository.productRepository.ToListAsync();
            var locations = await _dbRepository.storageLocationRepository.ToListAsync();
            var units = await _dbRepository.unitRepository.ToListAsync();

            return new StockLookups(
                products.ToDictionary(x => x.Id, x => (x.Name, x.MinStock)),
                locations.ToDictionary(x => x.Id, x => x.Name),
                units.ToDictionary(x => x.Id, x => x.ShortName));
        }

        private static StockBalanceDto MapToDto(StockBalance balance, StockLookups lookups)
        {
            var product = lookups.Products.TryGetValue(balance.ProductId, out var productInfo)
                ? productInfo
                : (Name: string.Empty, MinStock: 0m);

            return new StockBalanceDto
            {
                Id = balance.Id,
                ProductId = balance.ProductId,
                ProductName = product.Name,
                StorageLocationId = balance.StorageLocationId,
                StorageLocationName = lookups.LocationNames.TryGetValue(balance.StorageLocationId, out var locationName)
                    ? locationName
                    : string.Empty,
                UnitId = balance.UnitId,
                UnitName = lookups.UnitShortNames.TryGetValue(balance.UnitId, out var unitName)
                    ? unitName
                    : string.Empty,
                Quantity = balance.Quantity,
                MinStock = product.MinStock,
                IsDeficit = balance.Quantity < product.MinStock
            };
        }

        /// <summary>Справочники, нужные для отображения строк остатков.</summary>
        private sealed record StockLookups(
            IReadOnlyDictionary<Guid, (string Name, decimal MinStock)> Products,
            IReadOnlyDictionary<Guid, string> LocationNames,
            IReadOnlyDictionary<Guid, string> UnitShortNames);
    }
}
