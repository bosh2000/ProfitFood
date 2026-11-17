using ProfitFood.Applications.Dto.Menus;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IMenuRequirementAppService
    {
        /// <summary>Возвращает журнал меню-требований.</summary>
        Task<IReadOnlyCollection<MenuRequirementDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Возвращает документ вместе со строками.</summary>
        Task<MenuRequirementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Рассчитывает меню-требование по меню на дату. Возвращает идентификатор документа.</summary>
        Task<Guid> CalculateAsync(Guid dailyMenuId, CancellationToken cancellationToken = default);

        /// <summary>Сохраняет правки черновика.</summary>
        Task SaveAsync(MenuRequirementDto model, CancellationToken cancellationToken = default);

        /// <summary>Удаляет черновик требования.</summary>
        Task DeleteAsync(MenuRequirementDto model, CancellationToken cancellationToken = default);
    }
}
