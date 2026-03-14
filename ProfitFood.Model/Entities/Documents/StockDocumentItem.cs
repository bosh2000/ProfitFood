using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Documents
{
    /// <summary>
    /// Строка складского документа.
    /// </summary>
    public sealed class StockDocumentItem : EntityBase
    {
        public Guid StockDocumentId { get; set; } // FK складского документа
        public Guid ProductId { get; set; } // FK продукта
        public decimal Quantity { get; set; } // Количество
        public Guid UnitId { get; set; } // FK единицы
        public decimal Price { get; set; } // Цена
        public decimal Amount { get; set; } // Сумма

        public StockDocument StockDocument { get; set; } = null!; // Документ
        public Product Product { get; set; } = null!; // Продукт
        public Unit Unit { get; set; } = null!; // Единица
    }
}