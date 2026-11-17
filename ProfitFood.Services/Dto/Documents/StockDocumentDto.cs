using ProfitFood.Domain.Entities.Enums;

namespace ProfitFood.Applications.Dto.Documents
{
    /// <summary>Складской документ вместе со строками для формы заполнения.</summary>
    public sealed class StockDocumentDto
    {
        public Guid Id { get; set; }

        /// <summary>Тип документа: приход, выдача, списание или инвентаризация.</summary>
        public StockDocumentType DocumentType { get; set; }

        /// <summary>Номер документа; при создании присваивается автоматически.</summary>
        public string DocumentNumber { get; set; } = string.Empty;

        public DateOnly DocumentDate { get; set; }
        public Guid StorageLocationId { get; set; }
        public string StorageLocationName { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }

        /// <summary>Отображаемое имя статуса.</summary>
        public string StatusName => Status.ToString();

        public string? Notes { get; set; }

        /// <summary>Строки документа.</summary>
        public List<StockDocumentItemDto> Items { get; set; } = new();

        /// <summary>Признак того, что документ проведён и недоступен для правки.</summary>
        public bool IsPosted => Status == DocumentStatus.Posted;
    }
}
