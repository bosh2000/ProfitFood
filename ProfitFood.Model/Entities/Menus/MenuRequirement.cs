using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Меню-требование на выдачу продуктов.
    /// </summary>
    public sealed class MenuRequirement : EntityBase
    {
        public string Number { get; set; } = null!; // Номер документа
        public DateOnly RequirementDate { get; set; } // Дата документа
        public Guid DailyMenuId { get; set; } // FK меню
        public Guid StorageLocationId { get; set; } // FK склада
        public MenuRequirementStatus Status { get; set; } // Статус документа
        public int TotalChildrenCount { get; set; } // Общее количество детей
        public Guid? HeadSignerId { get; set; } // FK руководителя
        public Guid? StorekeeperSignerId { get; set; } // FK кладовщика
        public Guid? MedWorkerSignerId { get; set; } // FK медработника
        public string? Notes { get; set; } // Примечание
        public DateTime CreatedAt { get; set; } // Дата создания
        public DateTime? PrintedAt { get; set; } // Дата печати

        public DailyMenu DailyMenu { get; set; } = null!; // Меню на дату
        public StorageLocation StorageLocation { get; set; } = null!; // Склад
        public Signer? HeadSigner { get; set; } // Руководитель
        public Signer? StorekeeperSigner { get; set; } // Кладовщик
        public Signer? MedWorkerSigner { get; set; } // Медработник
        public ICollection<MenuRequirementItem> Items { get; set; } = new List<MenuRequirementItem>(); // Строки документа
        public ICollection<StockDocument> StockDocuments { get; set; } = new List<StockDocument>(); // Складские документы, созданные по требованию
    }
}