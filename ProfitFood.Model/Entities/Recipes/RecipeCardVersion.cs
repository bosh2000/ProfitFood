using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.Recipes
{
    /// <summary>
    /// Версия технологической карты.
    /// </summary>
    public sealed class RecipeCardVersion : ReferenceEntityBase
    {
        public Guid RecipeCardId { get; set; } // FK техкарты
        public int VersionNumber { get; set; } // Номер версии
        public DateOnly DateFrom { get; set; } // Начало действия версии
        public DateOnly? DateTo { get; set; } // Конец действия версии
        public decimal OutputQuantity { get; set; } // Выход блюда
        public Guid OutputUnitId { get; set; } // FK единицы выхода
        public string? Description { get; set; } // Комментарий к версии
        public bool IsApproved { get; set; } // Признак утверждения

        public RecipeCard RecipeCard { get; set; } = null!; // Техкарта
        public Unit OutputUnit { get; set; } = null!; // Единица выхода
        public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>(); // Ингредиенты
        public ICollection<DishOutput> DishOutputs { get; set; } = new List<DishOutput>(); // Выходы по возрастам
        public ICollection<CycleMenuItem> CycleMenuItems { get; set; } = new List<CycleMenuItem>(); // Использование в цикличном меню
        public ICollection<DailyMenuItem> DailyMenuItems { get; set; } = new List<DailyMenuItem>(); // Использование в меню на дату
    }
}