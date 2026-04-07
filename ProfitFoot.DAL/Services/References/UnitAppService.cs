using ProfitFood.Domain.ModelsViewModels;
using ProfitFood.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Infrastructure.Services.References
{
    public class UnitAppService : IUnitAppService
    {
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<UnitListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(UnitEditModel model, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<UnitListItemViewModel>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}