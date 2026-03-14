using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Позиция ежедневного меню.
    /// </summary>
    public sealed class DailyMenuItem : EntityBase
    {
        public Guid DailyMenuId { get; set; } // FK меню
        public Guid MealTypeId { get; set; } // FK приема пищи
        public Guid DishId { get; set; } // FK блюда
        public Guid RecipeCardVersionId { get; set; } // FK версии техкарты
        public int SortOrder { get; set; } // Порядок
        public string? Notes { get; set; } // Примечание

        public DailyMenu DailyMenu { get; set; } = null!; // Меню на дату
        public MealType MealType { get; set; } = null!; // Прием пищи
        public Dish Dish { get; set; } = null!; // Блюдо
        public RecipeCardVersion RecipeCardVersion { get; set; } = null!; // Версия техкарты
    }
}