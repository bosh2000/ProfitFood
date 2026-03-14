using ProfitFood.Domain.Entities.References;
using ProfitFood.Model.DBModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.Recipes
{
    /// <summary>
    /// Ингредиент техкарты.
    /// </summary>
    public sealed class RecipeIngredient : EntityBase
    {
        public Guid RecipeCardVersionId { get; set; } // FK версии техкарты
        public Guid ProductId { get; set; } // FK продукта
        public decimal GrossQuantity { get; set; } // Количество брутто
        public decimal NetQuantity { get; set; } // Количество нетто
        public Guid UnitId { get; set; } // FK единицы измерения
        public decimal LossPercent { get; set; } // Процент потерь
        public int SortOrder { get; set; } // Порядок отображения

        public RecipeCardVersion RecipeCardVersion { get; set; } = null!; // Версия техкарты
        public Product Product { get; set; } = null!; // Продукт
        public Unit Unit { get; set; } = null!; // Единица
    }
}