using AutoMapper;
using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Infrastructure.Repository.Interfaces;

namespace ProfitFood.Applications.Services.References
{
    public class UnitAppService : IUnitAppService
    {
        private readonly IDbRepository _dbRepository;
        private readonly IMapper _mapper;

        public UnitAppService(IDbRepository dbRepository, IMapper mapper)
        {
            _dbRepository = dbRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<UnitItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var unitListEntity = await _dbRepository.unitRepository.ToListAsync();

            return unitListEntity
                .OrderBy(x => x.Name)
                .Select(x => _mapper.Map<UnitItemDto>(x))
                .ToList();
        }

        public async Task<IReadOnlyCollection<UnitItemDto>> SearchAsync(
            string searchText,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllAsync(cancellationToken);

            var normalizedSearchText = searchText.Trim().ToLower();

            var units = await _dbRepository.unitRepository.ConditionToListAsync(x =>
                x.Name.ToLower().Contains(normalizedSearchText));

            return units
                .OrderBy(x => x.Name)
                .Select(x => _mapper.Map<UnitItemDto>(x))
                .ToList();
        }

        public async Task SaveAsync(
            UnitItemDto model,
            CancellationToken cancellationToken = default)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Наименование единицы измерения не может быть пустым.");

            var name = model.Name.Trim();

            if (model.Id == Guid.Empty)
            {
                var newEntity = _mapper.Map<Unit>(model);
                newEntity.Name = name;

                await _dbRepository.unitRepository.CreateASync(newEntity);
                return;
            }

            var existingEntity = await _dbRepository.unitRepository
                .FirstOfDefaultAsync(x => x.Id == model.Id);

            if (existingEntity is null)
            {
                var newEntity = _mapper.Map<Unit>(model);
                newEntity.Name = name;

                await _dbRepository.unitRepository.CreateASync(newEntity);
                return;
            }

            existingEntity.Name = name;

            await _dbRepository.unitRepository.UpdateAsync(existingEntity);
        }

        public async Task DeleteAsync(
            UnitItemDto model,
            CancellationToken cancellationToken = default)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var unitItemEntity = await _dbRepository.unitRepository
                .FirstOfDefaultAsync(x => x.Id == model.Id);

            if (unitItemEntity is null)
                throw new InvalidOperationException("Единица измерения не найдена.");

            await _dbRepository.unitRepository.DeleteAsync(unitItemEntity);
        }
    }
}