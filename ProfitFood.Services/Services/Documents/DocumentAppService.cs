using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.Documents;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Services.Documents
{
    /// <summary>
    /// Сервис раздела «Документы»: журнал складских документов и работа с их содержимым.
    /// </summary>
    public class DocumentAppService : IDocumentAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;
        private readonly ILogger<DocumentAppService> _logger;

        /// <summary>
        /// Инициализирует сервис документов.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям для чтения журнала.</param>
        /// <param name="contextFactory">
        /// Фабрика контекста: документ и его строки меняются в одном контексте,
        /// иначе сохранение распадается на независимые операции.
        /// </param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public DocumentAppService(
            IDbRepository dbRepository,
            IDbContextFactory<ProfitFoodDbContext> contextFactory,
            ILogger<DocumentAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает документы, начиная с самого свежего.</summary>
        public async Task<IReadOnlyCollection<StockDocumentListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var documents = await _dbRepository.stockDocumentRepository.ToListAsync();
            var locationNames = await LoadLocationNamesAsync();

            return documents
                .OrderByDescending(x => x.DocumentDate)
                .ThenByDescending(x => x.DocumentNumber)
                .Select(document => MapToDto(document, locationNames))
                .ToList();
        }

        /// <summary>Ищет документы по номеру.</summary>
        public async Task<IReadOnlyCollection<StockDocumentListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var documents = await _dbRepository.stockDocumentRepository
                .ConditionToListAsync(x => x.DocumentNumber.ToLower().Contains(normalizedSearchText));
            var locationNames = await LoadLocationNamesAsync();

            return documents
                .OrderByDescending(x => x.DocumentDate)
                .ThenByDescending(x => x.DocumentNumber)
                .Select(document => MapToDto(document, locationNames))
                .ToList();
        }

        /// <summary>Загружает документ вместе со строками.</summary>
        public async Task<StockDocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await context.StockDocuments
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null)
            {
                return null;
            }

            var lookups = await LoadLookupsAsync(context, cancellationToken);

            return MapToDto(entity, lookups);
        }

        /// <summary>Сохраняет документ вместе со строками. Возвращает идентификатор документа.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при некорректных данных или попытке изменить проведённый документ.</exception>
        public async Task<Guid> SaveAsync(StockDocumentDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (model.StorageLocationId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбран склад.");
            }

            ValidateItems(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                StockDocument entity;

                if (model.Id == Guid.Empty)
                {
                    entity = new StockDocument
                    {
                        DocumentType = model.DocumentType,
                        DocumentDate = model.DocumentDate,
                        StorageLocationId = model.StorageLocationId,
                        Status = DocumentStatus.Draft,
                        Notes = NormalizeNotes(model.Notes),
                        CreatedAt = DateTime.Now,
                        DocumentNumber = await GenerateNumberAsync(context, model.DocumentType, model.DocumentDate, cancellationToken)
                    };

                    context.StockDocuments.Add(entity);
                }
                else
                {
                    entity = await context.StockDocuments
                        .Include(x => x.Items)
                        .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                        ?? throw new InvalidOperationException("Документ не найден: возможно, его удалили в другом окне.");

                    if (entity.Status != DocumentStatus.Draft)
                    {
                        throw new InvalidOperationException("Проведённый документ нельзя изменить: сначала отмените проведение.");
                    }

                    entity.DocumentType = model.DocumentType;
                    entity.DocumentDate = model.DocumentDate;
                    entity.StorageLocationId = model.StorageLocationId;
                    entity.Notes = NormalizeNotes(model.Notes);
                }

                SyncItems(entity, model);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить складской документ {DocumentNumber}", model.DocumentNumber);
                throw;
            }
        }

        /// <summary>Удаляет документ вместе со строками.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если документ проведён или не найден.</exception>
        public async Task DeleteAsync(StockDocumentDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            try
            {
                var entity = await context.StockDocuments
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Документ не найден: возможно, его удалили в другом окне.");

                if (entity.Status != DocumentStatus.Draft)
                {
                    throw new InvalidOperationException("Проведённый документ нельзя удалить: сначала отмените проведение.");
                }

                // Строки удаляются каскадно: связь обязательная, EF считает их сиротами.
                context.StockDocuments.Remove(entity);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить складской документ {DocumentNumber}", model.DocumentNumber);
                throw;
            }
        }

        /// <summary>Проводит документ: применяет движение товара и переводит его в статус «Проведён».</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если документ не найден, уже проведён или не имеет строк.</exception>
        public async Task PostAsync(StockDocumentDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var document = await context.StockDocuments
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Документ не найден: возможно, его удалили в другом окне.");

                if (document.Status != DocumentStatus.Draft)
                {
                    throw new InvalidOperationException("Провести можно только документ в статусе «Черновик».");
                }

                if (document.Items.Count == 0)
                {
                    throw new InvalidOperationException("Нельзя провести документ без строк.");
                }

                await ApplyMovementsAsync(context, document, reverse: false, cancellationToken);

                document.Status = DocumentStatus.Posted;

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось провести документ {DocumentNumber}", model.DocumentNumber);
                throw;
            }
        }

        /// <summary>Отменяет проведение: возвращает остатки и переводит документ в «Черновик».</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если документ не проведён или отмена невозможна.</exception>
        public async Task UnpostAsync(StockDocumentDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var document = await context.StockDocuments
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Документ не найден: возможно, его удалили в другом окне.");

                if (document.Status != DocumentStatus.Posted)
                {
                    throw new InvalidOperationException("Отменить проведение можно только у проведённого документа.");
                }

                // Инвентаризация затирает прежнее количество без следа, поэтому обратный пересчёт невозможен.
                if (document.DocumentType == StockDocumentType.Inventory)
                {
                    throw new InvalidOperationException(
                        "Отмена проведения инвентаризации не поддерживается: фактический остаток уже применён.");
                }

                await ApplyMovementsAsync(context, document, reverse: true, cancellationToken);

                document.Status = DocumentStatus.Draft;

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось отменить проведение документа {DocumentNumber}", model.DocumentNumber);
                throw;
            }
        }

        /// <summary>
        /// Применяет движение товара по строкам документа. При <paramref name="reverse"/> = true движения
        /// выполняются в обратную сторону; инвентаризация обратному пересчёту не подлежит.
        /// </summary>
        /// <param name="context">Контекст, в котором уже открыта транзакция.</param>
        /// <param name="document">Документ со строками.</param>
        /// <param name="reverse">Признак обратного движения (отмена проведения).</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        private static async Task ApplyMovementsAsync(
            ProfitFoodDbContext context,
            StockDocument document,
            bool reverse,
            CancellationToken cancellationToken)
        {
            var products = await context.Products.ToDictionaryAsync(x => x.Id, cancellationToken);
            var units = await context.Units.ToDictionaryAsync(x => x.Id, cancellationToken);
            var balances = await context.StockBalances
                .Where(x => x.StorageLocationId == document.StorageLocationId)
                .ToListAsync(cancellationToken);

            foreach (var item in document.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    throw new InvalidOperationException("В строке документа указан продукт, которого нет в справочнике.");
                }

                if (!units.TryGetValue(item.UnitId, out var itemUnit))
                {
                    throw new InvalidOperationException($"Не найдена единица измерения для продукта «{product.Name}».");
                }

                if (!units.TryGetValue(product.BaseUnitId, out var baseUnit))
                {
                    throw new InvalidOperationException($"Не найдена базовая единица продукта «{product.Name}».");
                }

                // Пересчёт в базовую единицу продукта: остаток всегда хранится в ней.
                var factor = baseUnit.BaseFactor == 0m ? 1m : itemUnit.BaseFactor / baseUnit.BaseFactor;
                var quantity = item.Quantity * factor;

                var balance = balances.FirstOrDefault(x => x.ProductId == item.ProductId);

                if (balance is null)
                {
                    if (reverse)
                    {
                        throw new InvalidOperationException(
                            $"Не найден остаток продукта «{product.Name}» для отмены проведения.");
                    }

                    balance = new StockBalance
                    {
                        ProductId = item.ProductId,
                        StorageLocationId = document.StorageLocationId,
                        UnitId = product.BaseUnitId,
                        Quantity = 0m,
                        UpdatedAt = DateTime.Now
                    };

                    context.StockBalances.Add(balance);
                    balances.Add(balance);
                }

                switch (document.DocumentType)
                {
                    case StockDocumentType.Receipt:
                        ApplyDelta(balance, reverse ? -quantity : quantity, product.Name);
                        break;

                    case StockDocumentType.Issue:
                    case StockDocumentType.WriteOff:
                        ApplyDelta(balance, reverse ? quantity : -quantity, product.Name);
                        break;

                    case StockDocumentType.Inventory:
                        // Инвентаризация не прибавляет и не убавляет: она объявляет фактическое количество.
                        balance.Quantity = quantity;
                        balance.UpdatedAt = DateTime.Now;
                        break;

                    default:
                        throw new InvalidOperationException("Неизвестный тип складского документа.");
                }
            }
        }

        /// <summary>
        /// Изменяет остаток на величину <paramref name="delta"/> и запрещает уходить в минус.
        /// </summary>
        /// <param name="balance">Остаток продукта на складе.</param>
        /// <param name="delta">Прибавка или убавка в базовой единице.</param>
        /// <param name="productName">Наименование продукта для сообщения об ошибке.</param>
        /// <exception cref="InvalidOperationException">Возникает, если остаток становится отрицательным.</exception>
        private static void ApplyDelta(StockBalance balance, decimal delta, string productName)
        {
            var newQuantity = balance.Quantity + delta;

            if (newQuantity < 0m)
            {
                throw new InvalidOperationException(
                    $"Недостаточно продукта «{productName}» на складе: остаток {balance.Quantity:0.###}, движение {delta:0.###}.");
            }

            balance.Quantity = newQuantity;
            balance.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Строит номер документа: префикс по типу, год и порядковый номер внутри года.
        /// </summary>
        private static async Task<string> GenerateNumberAsync(
            ProfitFoodDbContext context,
            StockDocumentType documentType,
            DateOnly documentDate,
            CancellationToken cancellationToken)
        {
            var prefix = documentType switch
            {
                StockDocumentType.Receipt => "ПР",
                StockDocumentType.Issue => "ВД",
                StockDocumentType.WriteOff => "СП",
                StockDocumentType.Inventory => "ИНВ",
                _ => "ДК"
            };

            // Год сравнивается в памяти: DateOnly.Year в SQLite транслируется ненадёжно.
            var dates = await context.StockDocuments
                .Where(x => x.DocumentType == documentType)
                .Select(x => x.DocumentDate)
                .ToListAsync(cancellationToken);

            var numberInYear = dates.Count(x => x.Year == documentDate.Year) + 1;

            return $"{prefix}-{documentDate.Year}-{numberInYear:0000}";
        }

        /// <summary>
        /// Переносит строки из модели в сущность: обновляет существующие, добавляет новые и убирает лишние.
        /// </summary>
        private static void SyncItems(StockDocument entity, StockDocumentDto model)
        {
            var incomingIds = model.Items
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var removed in entity.Items.Where(x => !incomingIds.Contains(x.Id)).ToList())
            {
                entity.Items.Remove(removed);
            }

            foreach (var item in model.Items)
            {
                var target = item.Id == Guid.Empty
                    ? null
                    : entity.Items.FirstOrDefault(x => x.Id == item.Id);

                if (target is null)
                {
                    target = new StockDocumentItem { StockDocumentId = entity.Id };
                    entity.Items.Add(target);
                }

                target.ProductId = item.ProductId;
                target.UnitId = item.UnitId;
                target.Quantity = item.Quantity;
                target.Price = item.Price;
                target.Amount = item.Quantity * item.Price;
            }
        }

        /// <summary>
        /// Проверяет строки до сохранения: без продукта или единицы строка бессмысленна,
        /// а нулевое или отрицательное количество не даёт движения.
        /// </summary>
        private static void ValidateItems(StockDocumentDto model)
        {
            if (model.Items.Count == 0)
            {
                return;
            }

            foreach (var item in model.Items)
            {
                if (item.ProductId == Guid.Empty)
                {
                    throw new InvalidOperationException("В строке документа не выбран продукт.");
                }

                if (item.UnitId == Guid.Empty)
                {
                    throw new InvalidOperationException("В строке документа не выбрана единица измерения.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException("Количество в строке документа должно быть больше нуля.");
                }
            }
        }

        private static string? NormalizeNotes(string? notes)
            => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        /// <summary>
        /// Загружает справочники «идентификатор — наименование» для отображения строк документа.
        /// </summary>
        private static async Task<DocumentLookups> LoadLookupsAsync(ProfitFoodDbContext context, CancellationToken cancellationToken)
        {
            var products = await context.Products.AsNoTracking().ToListAsync(cancellationToken);
            var units = await context.Units.AsNoTracking().ToListAsync(cancellationToken);
            var locations = await context.StorageLocations.AsNoTracking().ToListAsync(cancellationToken);

            return new DocumentLookups(
                products.ToDictionary(x => x.Id, x => x.Name),
                units.ToDictionary(x => x.Id, x => x.Name),
                locations.ToDictionary(x => x.Id, x => x.Name));
        }

        private static StockDocumentDto MapToDto(StockDocument document, DocumentLookups lookups) => new()
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            DocumentNumber = document.DocumentNumber,
            DocumentDate = document.DocumentDate,
            StorageLocationId = document.StorageLocationId,
            StorageLocationName = lookups.LocationNames.TryGetValue(document.StorageLocationId, out var locationName)
                ? locationName
                : string.Empty,
            Status = document.Status,
            Notes = document.Notes,
            Items = document.Items
                .OrderBy(x => x.ProductId)
                .Select(item => new StockDocumentItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = lookups.ProductNames.TryGetValue(item.ProductId, out var productName)
                        ? productName
                        : string.Empty,
                    UnitId = item.UnitId,
                    UnitName = lookups.UnitNames.TryGetValue(item.UnitId, out var unitName)
                        ? unitName
                        : string.Empty,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Amount = item.Amount
                })
                .ToList()
        };

        /// <summary>
        /// Загружает словарь «идентификатор склада — наименование» одним запросом.
        /// </summary>
        private async Task<IReadOnlyDictionary<Guid, string>> LoadLocationNamesAsync()
        {
            var locations = await _dbRepository.storageLocationRepository.ToListAsync();

            return locations.ToDictionary(x => x.Id, x => x.Name);
        }

        private static StockDocumentListItemDto MapToDto(
            StockDocument document,
            IReadOnlyDictionary<Guid, string> locationNames) => new()
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                DocumentTypeName = document.DocumentType.ToString(),
                StorageLocationName = locationNames.TryGetValue(document.StorageLocationId, out var locationName)
                    ? locationName
                    : string.Empty,
                StatusName = document.Status.ToString()
            };

        /// <summary>Справочники, нужные для отображения строк документа.</summary>
        private sealed record DocumentLookups(
            IReadOnlyDictionary<Guid, string> ProductNames,
            IReadOnlyDictionary<Guid, string> UnitNames,
            IReadOnlyDictionary<Guid, string> LocationNames);
    }
}
