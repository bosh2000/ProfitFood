using ProfitFood.Domain.Entities.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Конкретная группа детей в ДОУ.
    /// </summary>
    public sealed class ChildGroup : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Наименование группы
        public Guid AgeGroupId { get; set; } // FK возрастной группы
        public int PlannedChildrenCount { get; set; } // Плановое количество детей

        public AgeGroup AgeGroup { get; set; } = null!; // Навигация к возрастной группе
        public ICollection<DailyMenuAttendance> DailyMenuAttendances { get; set; } = new List<DailyMenuAttendance>(); // Посещаемость по дням
    }
}