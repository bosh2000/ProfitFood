using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Посещаемость по группе детей в рамках меню.
    /// </summary>
    public sealed class DailyMenuAttendance : EntityBase
    {
        public Guid DailyMenuId { get; set; } // FK меню
        public Guid ChildGroupId { get; set; } // FK группы детей
        public int PlannedCount { get; set; } // Плановое количество
        public int ActualCount { get; set; } // Фактическое количество

        public DailyMenu DailyMenu { get; set; } = null!; // Меню на дату
        public ChildGroup ChildGroup { get; set; } = null!; // Группа детей
    }
}