using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Строка меню-требования.
    /// </summary>
    public sealed class MenuRequirementItem : EntityBase
    {
        public Guid MenuRequirementId { get; set; } // FK меню-требования
        public Guid ProductId { get; set; } // FK продукта
        public Guid UnitId { get; set; } // FK единицы измерения
        public decimal Quantity { get; set; } // Количество
        public decimal Price { get; set; } // Цена
        public decimal Amount { get; set; } // Сумма
        public int SortOrder { get; set; } // Порядок

        public MenuRequirement MenuRequirement { get; set; } = null!; // Документ
        public Product Product { get; set; } = null!; // Продукт
        public Unit Unit { get; set; } = null!; // Единица
    }
}