using Microsoft.Extensions.Logging;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Infrastructure.Services.References
{
    /// <summary>
    /// Сервис справочника групп детей в ДОУ.
    /// </summary>
    public class ChildGroupAppService : IChildGroupAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<ChildGroupAppService> _logger;

        /// <summary>
        /// Инициализирует сервис справочника групп детей.
        /// </summary>
        /// <param name="dbRepository">Доступ к репозиториям.</param>
        /// <param name="logger">Журнал приложения.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public ChildGroupAppService(IDbRepository dbRepository, ILogger<ChildGroupAppService> logger)
        {
            _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Возвращает все группы детей с наименованиями возрастных групп.</summary>
        public async Task<IReadOnlyCollection<ChildGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbRepository.childGroupRepository.ToListAsync();
            var ageGroupNames = await LoadAgeGroupNamesAsync();

            return entities
                .OrderBy(x => x.Name)
                .Select(entity => MapToDto(entity, ageGroupNames))
                .ToList();
        }

        /// <summary>Ищет группы детей по части наименования.</summary>
        public async Task<IReadOnlyCollection<ChildGroupDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync(cancellationToken);
            }

            var normalizedSearchText = searchText.Trim().ToLowerInvariant();

            var entities = await _dbRepository.childGroupRepository
                .ConditionToListAsync(x => x.Name.ToLower().Contains(normalizedSearchText));
            var ageGroupNames = await LoadAgeGroupNamesAsync();

            return entities
                .OrderBy(x => x.Name)
                .Select(entity => MapToDto(entity, ageGroupNames))
                .ToList();
        }

        /// <summary>Сохраняет группу детей: создаёт новую или обновляет существующую.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает при пустом наименовании, неверном количестве детей или отсутствии возрастной группы.</exception>
        public async Task SaveAsync(ChildGroupDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            var name = model.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Наименование группы детей не может быть пустым.");
            }

            if (model.PlannedChildrenCount < 0)
            {
                throw new InvalidOperationException("Плановое количество детей не может быть отрицательным.");
            }

            if (model.AgeGroupId == Guid.Empty)
            {
                throw new InvalidOperationException("Не выбрана возрастная группа.");
            }

            try
            {
                var ageGroup = await _dbRepository.ageGroupRepository
                    .FirstOfDefaultAsync(x => x.Id == model.AgeGroupId);

                if (ageGroup is null)
                {
                    throw new InvalidOperationException("Выбранная возрастная группа не найдена.");
                }

                if (model.Id == Guid.Empty)
                {
                    var newEntity = new ChildGroup();
                    Apply(newEntity, model, name);
                    await _dbRepository.childGroupRepository.CreateASync(newEntity);
                    return;
                }

                var existingEntity = await _dbRepository.childGroupRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (existingEntity is null)
                {
                    throw new InvalidOperationException("Группа детей не найдена: возможно, её удалили в другом окне.");
                }

                Apply(existingEntity, model, name);
                await _dbRepository.childGroupRepository.UpdateAsync(existingEntity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось сохранить группу детей «{Name}»", name);
                throw;
            }
        }

        /// <summary>Удаляет группу детей.</summary>
        /// <exception cref="ArgumentNullException">Возникает, если модель пуста.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если запись не найдена.</exception>
        public async Task DeleteAsync(ChildGroupDto model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                var entity = await _dbRepository.childGroupRepository
                    .FirstOfDefaultAsync(x => x.Id == model.Id);

                if (entity is null)
                {
                    throw new InvalidOperationException("Группа детей не найдена.");
                }

                await _dbRepository.childGroupRepository.DeleteAsync(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось удалить группу детей с идентификатором {Id}", model.Id);
                throw;
            }
        }

        /// <summary>
        /// Загружает словарь «идентификатор возрастной группы — наименование» одним запросом,
        /// чтобы не обращаться к базе для каждой строки списка.
        /// </summary>
        private async Task<IReadOnlyDictionary<Guid, string>> LoadAgeGroupNamesAsync()
        {
            var ageGroups = await _dbRepository.ageGroupRepository.ToListAsync();

            return ageGroups.ToDictionary(x => x.Id, x => x.Name);
        }

        private static ChildGroupDto MapToDto(ChildGroup entity, IReadOnlyDictionary<Guid, string> ageGroupNames) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            AgeGroupId = entity.AgeGroupId,
            AgeGroupName = ageGroupNames.TryGetValue(entity.AgeGroupId, out var name) ? name : string.Empty,
            PlannedChildrenCount = entity.PlannedChildrenCount
        };

        private static void Apply(ChildGroup entity, ChildGroupDto model, string name)
        {
            entity.Name = name;
            entity.AgeGroupId = model.AgeGroupId;
            entity.PlannedChildrenCount = model.PlannedChildrenCount;
        }
    }
}
