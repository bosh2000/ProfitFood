using ProfitFood.Applications.Dto.Documents;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IDocumentAppService
    {
        Task<IReadOnlyCollection<StockDocumentListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<StockDocumentListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        /// <summary>Загружает документ вместе со строками.</summary>
        Task<StockDocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Сохраняет документ вместе со строками. Возвращает идентификатор документа.</summary>
        Task<Guid> SaveAsync(StockDocumentDto model, CancellationToken cancellationToken = default);

        /// <summary>Удаляет документ вместе со строками.</summary>
        Task DeleteAsync(StockDocumentDto model, CancellationToken cancellationToken = default);

        /// <summary>Проводит документ: применяет движение товара и переводит его в статус «Проведён».</summary>
        Task PostAsync(StockDocumentDto model, CancellationToken cancellationToken = default);

        /// <summary>Отменяет проведение: возвращает остатки и переводит документ в «Черновик».</summary>
        Task UnpostAsync(StockDocumentDto model, CancellationToken cancellationToken = default);
    }
}
