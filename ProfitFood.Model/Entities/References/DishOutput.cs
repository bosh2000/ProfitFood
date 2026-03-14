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
    /// Выход блюда по возрастной группе.
    /// </summary>
    public sealed class DishOutput : EntityBase
    {
        public Guid RecipeCardVersionId { get; set; } // FK версии техкарты
        public Guid AgeGroupId { get; set; } // FK возрастной группы
        public decimal OutputQuantity { get; set; } // Нормативный выход
        public Guid UnitId { get; set; } // FK единицы измерения

        public RecipeCardVersion RecipeCardVersion { get; set; } = null!; // Версия техкарты
        public AgeGroup AgeGroup { get; set; } = null!; // Возрастная группа
        public Unit Unit { get; set; } = null!; // Единица выхода
    }
}