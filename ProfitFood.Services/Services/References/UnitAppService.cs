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

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<UnitItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var unitListEntity = await _dbRepository.unitRepository.ToListAsync();
            return unitListEntity.Select(x => _mapper.Map<UnitItemDto>(x)).ToList();
        }

        public Task SaveAsync(UnitEditDto model, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<UnitItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}