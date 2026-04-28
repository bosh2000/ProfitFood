using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Dto.References.ProfitFood.Applications.Dto.References;
using ProfitFood.UI.Models.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IProductCategoryAppService
    {
        Task<IReadOnlyCollection<ProductCategoryTreeDto>> GetTreeAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ProductCategoryLookupDto>> GetLookupAsync(
            CancellationToken cancellationToken = default);

        Task SaveAsync(
            ProductCategorySaveDto dto,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> CanMoveAsync(
            Guid categoryId,
            Guid? newParentCategoryId,
            CancellationToken cancellationToken = default);
    }
}