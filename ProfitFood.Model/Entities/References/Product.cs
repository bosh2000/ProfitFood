using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Продукт питания.
    /// </summary>
    public sealed class Product : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Краткое название
        public string FullName { get; set; } = null!; // Полное название
        public Guid ProductCategoryId { get; set; } // FK категории
        public Guid BaseUnitId { get; set; } // FK базовой единицы
        public Guid? StorageUnitId { get; set; } // FK складской единицы
        public string? Article { get; set; } // Код/артикул
        public int SortOrder { get; set; } // Сортировка
        public bool IsPerishable { get; set; } // Скоропортящийся продукт
        public decimal MinStock { get; set; } // Минимальный остаток

        public ProductCategory ProductCategory { get; set; } = null!; // Навигация к категории
        public Unit BaseUnit { get; set; } = null!; // Навигация к базовой единице
        public Unit? StorageUnit { get; set; } // Навигация к складской единице

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>(); // Использование в рецептах
        public ICollection<MenuRequirementItem> MenuRequirementItems { get; set; } = new List<MenuRequirementItem>(); // Использование в меню-требованиях
        public ICollection<StockDocumentItem> StockDocumentItems { get; set; } = new List<StockDocumentItem>(); // Использование в складских документах
        public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>(); // Остатки по продукту
    }
}