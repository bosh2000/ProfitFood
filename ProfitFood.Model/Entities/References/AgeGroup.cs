using ProfitFood.Domain.Entities.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Возрастная группа детей.
    /// </summary>
    public sealed class AgeGroup : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Наименование возрастной группы
        public int AgeFromMonths { get; set; } // Возраст от
        public int AgeToMonths { get; set; } // Возраст до

        public ICollection<ChildGroup> ChildGroups { get; set; } = new List<ChildGroup>(); // Группы детей
        public ICollection<DishOutput> DishOutputs { get; set; } = new List<DishOutput>(); // Выходы блюд
        public ICollection<CycleMenu> CycleMenus { get; set; } = new List<CycleMenu>(); // Цикличные меню
    }
}