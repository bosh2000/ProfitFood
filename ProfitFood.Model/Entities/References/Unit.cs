using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Model.DBModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Единица измерения.
    /// </summary>
    public sealed class Unit : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Полное название
        public string ShortName { get; set; } = null!; // Краткое название
        public UnitType UnitType { get; set; } // Тип единицы
        public decimal BaseFactor { get; set; } // Коэффициент пересчета к базовой единице
        public bool IsBase { get; set; } // Является ли единица базовой

        public ICollection<Product> BaseProducts { get; set; } = new List<Product>(); // Продукты, где единица базовая
        public ICollection<Product> StorageProducts { get; set; } = new List<Product>(); // Продукты, где единица складская
        public ICollection<RecipeCardVersion> OutputRecipeVersions { get; set; } = new List<RecipeCardVersion>(); // Версии техкарт с выходом в этой единице
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>(); // Ингредиенты в этой единице
        public ICollection<DishOutput> DishOutputs { get; set; } = new List<DishOutput>(); // Выходы блюда по возрастам
        public ICollection<MenuRequirementItem> MenuRequirementItems { get; set; } = new List<MenuRequirementItem>(); // Строки меню-требований
        public ICollection<StockDocumentItem> StockDocumentItems { get; set; } = new List<StockDocumentItem>(); // Строки складских документов
        public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>(); // Остатки
    }
}