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
    /// Блюдо.
    /// </summary>
    public sealed class Dish : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Название блюда
        public Guid DishCategoryId { get; set; } // FK категории блюда
        public Guid MealTypeId { get; set; } // FK приема пищи
        public string Code { get; set; } = null!; // Код блюда
        public string? Notes { get; set; } // Примечание

        public DishCategory DishCategory { get; set; } = null!; // Категория блюда
        public MealType MealType { get; set; } = null!; // Прием пищи
        public ICollection<RecipeCard> RecipeCards { get; set; } = new List<RecipeCard>(); // Техкарты блюда
        public ICollection<CycleMenuItem> CycleMenuItems { get; set; } = new List<CycleMenuItem>(); // Позиции шаблонов меню
        public ICollection<DailyMenuItem> DailyMenuItems { get; set; } = new List<DailyMenuItem>(); // Позиции меню на дату
    }
}