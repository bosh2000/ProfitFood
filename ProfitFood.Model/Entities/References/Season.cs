using ProfitFood.Domain.Entities.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Сезонность меню.
    /// </summary>
    public sealed class Season : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Название сезона
        public int MonthFrom { get; set; } // Начальный месяц
        public int MonthTo { get; set; } // Конечный месяц

        public ICollection<CycleMenu> CycleMenus { get; set; } = new List<CycleMenu>(); // Цикличные меню сезона
        public ICollection<DailyMenu> DailyMenus { get; set; } = new List<DailyMenu>(); // Меню на дату сезона
    }
}