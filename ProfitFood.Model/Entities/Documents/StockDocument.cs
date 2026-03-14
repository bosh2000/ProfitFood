using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Documents
{
    /// <summary>
    /// Складской документ движения продуктов.
    /// </summary>
    public sealed class StockDocument : EntityBase
    {
        public StockDocumentType DocumentType { get; set; } // Тип документа
        public string DocumentNumber { get; set; } = null!; // Номер документа
        public DateOnly DocumentDate { get; set; } // Дата документа
        public Guid StorageLocationId { get; set; } // FK склада
        public Guid? RelatedMenuRequirementId { get; set; } // FK меню-требования
        public DocumentStatus Status { get; set; } // Статус
        public string? Notes { get; set; } // Примечание
        public DateTime CreatedAt { get; set; } // Дата создания

        public StorageLocation StorageLocation { get; set; } = null!; // Склад
        public MenuRequirement? RelatedMenuRequirement { get; set; } // Основание выдачи
        public ICollection<StockDocumentItem> Items { get; set; } = new List<StockDocumentItem>(); // Строки документа
    }
}