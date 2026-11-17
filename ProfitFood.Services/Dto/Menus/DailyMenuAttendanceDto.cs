namespace ProfitFood.Applications.Dto.Menus
{
    /// <summary>Строка посещаемости по группе детей на дату меню.</summary>
    public sealed class DailyMenuAttendanceDto
    {
        /// <summary>Идентификатор группы детей.</summary>
        public Guid ChildGroupId { get; set; }

        /// <summary>Наименование группы детей.</summary>
        public string ChildGroupName { get; set; } = string.Empty;

        /// <summary>Плановое количество детей.</summary>
        public int PlannedCount { get; set; }

        /// <summary>Фактическое количество детей.</summary>
        public int ActualCount { get; set; }
    }
}
