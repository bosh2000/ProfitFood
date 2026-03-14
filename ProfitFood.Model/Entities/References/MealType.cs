using ProfitFood.Domain.Entities.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Тип приема пищи.(завтрак, обед).
    /// </summary>
    public sealed class MealType : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Наименование приема пищи
        public string Code { get; set; } = null!; // Код приема пищи
        public int SortOrder { get; set; } // Сортировка

        public ICollection<Dish> Dishes { get; set; } = new List<Dish>(); // Блюда по умолчанию
        public ICollection<CycleMenuItem> CycleMenuItems { get; set; } = new List<CycleMenuItem>(); // Позиции цикличного меню
        public ICollection<DailyMenuItem> DailyMenuItems { get; set; } = new List<DailyMenuItem>(); // Позиции ежедневного меню
    }
}