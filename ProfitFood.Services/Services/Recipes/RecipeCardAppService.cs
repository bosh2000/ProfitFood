using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.Recipes;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFoot.Infrastructure;

namespace ProfitFood.Infrastructure.Services.Recipes
{
    /// <summary>
    /// Сервис раздела «Техкарты»: журнал, заполнение версии и ингредиентов.
    /// </summary>
    public class RecipeCardAppService : IRecipeCardAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly IDbContextFactory<ProfitFoodDbContext> _contextFactory;
        private readonly ILogger<RecipeCardAppService> _logger;

        /// <summary>
        /// Инициализирует сервис технологических карт.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям для чтения журнала.</param>
        /// <param name="contextFactory">
        /// Фабрика контекста: карта, версия и ингредиенты сохраняются в одном контексте и одной транзакции.
        /// </param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public RecipeCardAppService(
            IDbRepository dbRepository,
            IDbContextFactory<ProfitFoodDbContext> contextFactory,
            ILogger<RecipeCardAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все техкарты с данными последней версии.</summary>
        public async Task<IReadOnlyCollection<RecipeCardListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cards = await _dbRepository.recipeCardRepository.ToListAsync();
            var versions = await _dbRepository.recipeCardVersionRepository.ToListAsync();
            var unitNames = await LoadUnitNamesAsync();

            return cards
                .OrderBy(x => x.CardNumber)
                .Select(card => MapToDto(card, versions, unitNames))
                .ToList();
        }

        /// <summary>Ищет техкарты по номеру или наименованию.</summary>
        public async Task<IReadOnlyCollection<RecipeCardListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var cards = await _dbRepository.recipeCardRepository
                .ConditionToListAsync(x => x.CardNumber.ToLower().Contains(normalizedSearchText)
                                           || x.Name.ToLower().Contains(normalizedSearchText));
            var versions = await _dbRepository.recipeCardVersionRepository.ToListAsync();
            var unitNames = await LoadUnitNamesAsync();

            return cards
                .OrderBy(x => x.CardNumber)
                .Select(card => MapToDto(card, versions, unitNames))
                .ToList();
        }

        /// <summary>Загружает техкарту вместе с редактируемой версией и ингредиентами.</summary>
        public async Task<RecipeCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var card = await context.RecipeCards
                .AsNoTracking()
                .Include(x => x.Versions)
                .ThenInclude(version => version.Ingredients)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (card is null)
            {
                return null;
            }

            var lookups = await LoadLookupsAsync(context, cancellationToken);

            return MapToDetailDto(card, lookups);
        }

        /// <summary>Сохраняет техкарту, её версию и ингредиенты.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при некорректных данных или отсутствии связанных записей.</exception>
        public async Task<Guid> SaveAsync(RecipeCardDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            Validate(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var dishExists = await context.Dishes.AnyAsync(x => x.Id == model.DishId, cancellationToken);
                if (!dishExists)
                {
                    throw new InvalidOperationException("Выбранное блюдо не найдено.");
                }

                var unitExists = await context.Units.AnyAsync(x => x.Id == model.Version.OutputUnitId, cancellationToken);
                if (!unitExists)
                {
                    throw new InvalidOperationException("Выбранная единица выхода не найдена.");
                }

                RecipeCard card;

                if (model.Id == Guid.Empty)
                {
                    card = new RecipeCard
                    {
                        DishId = model.DishId,
                        CardNumber = model.CardNumber.Trim(),
                        Name = model.Name.Trim()
                    };

                    context.RecipeCards.Add(card);
                }
                else
                {
                    card = await context.RecipeCards
                        .Include(x => x.Versions)
                        .ThenInclude(version => version.Ingredients)
                        .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                        ?? throw new InvalidOperationException("Техкарта не найдена: возможно, её удалили в другом окне.");

                    card.DishId = model.DishId;
                    card.CardNumber = model.CardNumber.Trim();
                    card.Name = model.Name.Trim();
                }

                ApplyVersion(context, card, model.Version);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return card.Id;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить техкарту {CardNumber}", model.CardNumber);
                throw;
            }
        }

        /// <summary>Удаляет техкарту вместе с версиями и ингредиентами.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если техкарта не найдена.</exception>
        public async Task DeleteAsync(RecipeCardDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            try
            {
                var card = await context.RecipeCards
                    .Include(x => x.Versions)
                    .ThenInclude(version => version.Ingredients)
                    .FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Техкарта не найдена: возможно, её удалили в другом окне.");

                context.RecipeCards.Remove(card);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить техкарту {CardNumber}", model.CardNumber);
                throw;
            }
        }

        /// <summary>
        /// Применяет версию из модели: обновляет существующую или создаёт новую с очередным номером.
        /// </summary>
        private static void ApplyVersion(ProfitFoodDbContext context, RecipeCard card, RecipeCardVersionDto versionModel)
        {
            RecipeCardVersion version;

            if (versionModel.Id == Guid.Empty)
            {
                var nextNumber = card.Versions.Count == 0
                    ? 1
                    : card.Versions.Max(x => x.VersionNumber) + 1;

                version = new RecipeCardVersion
                {
                    RecipeCardId = card.Id,
                    VersionNumber = versionModel.VersionNumber > 0 ? versionModel.VersionNumber : nextNumber
                };

                card.Versions.Add(version);
            }
            else
            {
                version = card.Versions.FirstOrDefault(x => x.Id == versionModel.Id)
                          ?? throw new InvalidOperationException("Версия техкарты не найдена: возможно, её удалили в другом окне.");
            }

            version.DateFrom = versionModel.DateFrom;
            version.DateTo = versionModel.DateTo;
            version.OutputQuantity = versionModel.OutputQuantity;
            version.OutputUnitId = versionModel.OutputUnitId;
            version.Description = string.IsNullOrWhiteSpace(versionModel.Description) ? null : versionModel.Description.Trim();
            version.IsApproved = versionModel.IsApproved;

            SyncIngredients(version, versionModel);
        }

        /// <summary>
        /// Синхронизирует ингредиенты версии: обновляет существующие, добавляет новые и убирает лишние.
        /// </summary>
        private static void SyncIngredients(RecipeCardVersion version, RecipeCardVersionDto versionModel)
        {
            var incomingIds = versionModel.Ingredients
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var removed in version.Ingredients.Where(x => !incomingIds.Contains(x.Id)).ToList())
            {
                version.Ingredients.Remove(removed);
            }

            var sortOrder = 0;

            foreach (var ingredient in versionModel.Ingredients)
            {
                var target = ingredient.Id == Guid.Empty
                    ? null
                    : version.Ingredients.FirstOrDefault(x => x.Id == ingredient.Id);

                if (target is null)
                {
                    target = new RecipeIngredient { RecipeCardVersionId = version.Id };
                    version.Ingredients.Add(target);
                }

                target.ProductId = ingredient.ProductId;
                target.UnitId = ingredient.UnitId;
                target.GrossQuantity = ingredient.GrossQuantity;
                target.NetQuantity = ingredient.NetQuantity;
                target.LossPercent = ingredient.LossPercent;
                target.SortOrder = ingredient.SortOrder > 0 ? ingredient.SortOrder : sortOrder;

                sortOrder++;
            }
        }

        /// <summary>
        /// Проверяет обязательные поля карты и версии до открытия транзакции.
        /// </summary>
        private static void Validate(RecipeCardDto model)
        {
            if (string.IsNullOrWhiteSpace(model.CardNumber))
            {
                throw new InvalidOperationException("Номер техкарты не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new InvalidOperationException("Наименование техкарты не может быть пустым.");
            }

            if (model.DishId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрано блюдо.");
            }

            if (model.Version.OutputQuantity <= 0m)
            {
                throw new InvalidOperationException("Выход блюда должен быть больше нуля.");
            }

            if (model.Version.OutputUnitId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрана единица выхода.");
            }

            foreach (var ingredient in model.Version.Ingredients)
            {
                if (ingredient.ProductId == Guid.Empty)
                {
                    throw new InvalidOperationException("В строке ингредиента не выбран продукт.");
                }

                if (ingredient.UnitId == Guid.Empty)
                {
                    throw new InvalidOperationException("В строке ингредиента не выбрана единица измерения.");
                }

                if (ingredient.NetQuantity <= 0m)
                {
                    throw new InvalidOperationException("Количество нетто в строке ингредиента должно быть больше нуля.");
                }
            }
        }

        /// <summary>
        /// Загружает справочники, нужные для отображения технологической карты.
        /// </summary>
        private static async Task<RecipeLookups> LoadLookupsAsync(ProfitFoodDbContext context, CancellationToken cancellationToken)
        {
            var dishNames = await context.Dishes
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var productNames = await context.Products
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var unitNames = await context.Units
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return new RecipeLookups(dishNames, productNames, unitNames);
        }

        private static RecipeCardDto MapToDetailDto(RecipeCard card, RecipeLookups lookups)
        {
            // Актуальной считается версия с самой поздней датой начала действия.
            var currentVersion = card.Versions
                .OrderByDescending(x => x.DateFrom)
                .ThenByDescending(x => x.VersionNumber)
                .FirstOrDefault();

            var dto = new RecipeCardDto
            {
                Id = card.Id,
                CardNumber = card.CardNumber,
                Name = card.Name,
                DishId = card.DishId,
                DishName = lookups.DishNames.TryGetValue(card.DishId, out var dishName) ? dishName : string.Empty
            };

            if (currentVersion is null)
            {
                return dto;
            }

            dto.Version = new RecipeCardVersionDto
            {
                Id = currentVersion.Id,
                VersionNumber = currentVersion.VersionNumber,
                DateFrom = currentVersion.DateFrom,
                DateTo = currentVersion.DateTo,
                OutputQuantity = currentVersion.OutputQuantity,
                OutputUnitId = currentVersion.OutputUnitId,
                OutputUnitName = lookups.UnitNames.TryGetValue(currentVersion.OutputUnitId, out var unitName)
                    ? unitName
                    : string.Empty,
                Description = currentVersion.Description,
                IsApproved = currentVersion.IsApproved,
                Ingredients = currentVersion.Ingredients
                    .OrderBy(x => x.SortOrder)
                    .Select(ingredient => new RecipeIngredientDto
                    {
                        Id = ingredient.Id,
                        ProductId = ingredient.ProductId,
                        ProductName = lookups.ProductNames.TryGetValue(ingredient.ProductId, out var productName)
                            ? productName
                            : string.Empty,
                        UnitId = ingredient.UnitId,
                        UnitName = lookups.UnitNames.TryGetValue(ingredient.UnitId, out var ingredientUnitName)
                            ? ingredientUnitName
                            : string.Empty,
                        GrossQuantity = ingredient.GrossQuantity,
                        NetQuantity = ingredient.NetQuantity,
                        LossPercent = ingredient.LossPercent,
                        SortOrder = ingredient.SortOrder
                    })
                    .ToList()
            };

            return dto;
        }

        /// <summary>
        /// Загружает словарь «идентификатор единицы — наименование» одним запросом.
        /// </summary>
        private async Task<IReadOnlyDictionary<Guid, string>> LoadUnitNamesAsync()
        {
            var units = await _dbRepository.unitRepository.ToListAsync();

            return units.ToDictionary(x => x.Id, x => x.Name);
        }

        private static RecipeCardListItemDto MapToDto(
            RecipeCard card,
            IEnumerable<RecipeCardVersion> versions,
            IReadOnlyDictionary<Guid, string> unitNames)
        {
            var currentVersion = versions
                .Where(x => x.RecipeCardId == card.Id)
                .OrderByDescending(x => x.DateFrom)
                .ThenByDescending(x => x.VersionNumber)
                .FirstOrDefault();

            return new RecipeCardListItemDto
            {
                Id = card.Id,
                CardNumber = card.CardNumber,
                Name = card.Name,
                VersionNumber = currentVersion?.VersionNumber,
                VersionDateFrom = currentVersion?.DateFrom,
                OutputQuantity = currentVersion?.OutputQuantity,
                OutputUnitName = currentVersion is not null
                                  && unitNames.TryGetValue(currentVersion.OutputUnitId, out var unitName)
                    ? unitName
                    : string.Empty,
                IsApproved = currentVersion?.IsApproved ?? false
            };
        }

        /// <summary>Справочники, нужные для отображения технологической карты.</summary>
        private sealed record RecipeLookups(
            IReadOnlyDictionary<Guid, string> DishNames,
            IReadOnlyDictionary<Guid, string> ProductNames,
            IReadOnlyDictionary<Guid, string> UnitNames);
    }
}
