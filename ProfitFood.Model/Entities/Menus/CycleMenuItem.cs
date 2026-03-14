using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Позиция цикличного меню.
    /// </summary>
    public sealed class CycleMenuItem : EntityBase
    {
        public Guid CycleMenuDayId { get; set; } // FK дня цикличного меню
        public Guid MealTypeId { get; set; } // FK приема пищи
        public Guid DishId { get; set; } // FK блюда
        public Guid? RecipeCardVersionId { get; set; } // FK версии техкарты
        public int SortOrder { get; set; } // Порядок внутри приема пищи

        public CycleMenuDay CycleMenuDay { get; set; } = null!; // День шаблона
        public MealType MealType { get; set; } = null!; // Прием пищи
        public Dish Dish { get; set; } = null!; // Блюдо
        public RecipeCardVersion? RecipeCardVersion { get; set; } // Версия техкарты
    }
}