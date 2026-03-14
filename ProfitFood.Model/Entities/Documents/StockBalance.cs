using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Documents
{
    /// <summary>
    /// Остаток продукта на складе.
    /// </summary>
    public sealed class StockBalance : EntityBase
    {
        public Guid StorageLocationId { get; set; } // FK склада
        public Guid ProductId { get; set; } // FK продукта
        public decimal Quantity { get; set; } // Остаток
        public Guid UnitId { get; set; } // FK единицы
        public DateTime UpdatedAt { get; set; } // Дата последнего обновления

        public StorageLocation StorageLocation { get; set; } = null!; // Склад
        public Product Product { get; set; } = null!; // Продукт
        public Unit Unit { get; set; } = null!; // Единица
    }
}