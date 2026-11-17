namespace ProfitFood.Applications.Dto.Menus
{
    /// <summary>Строка списка ежедневных меню.</summary>
    public sealed class DailyMenuListItemDto
    {
        public Guid Id { get; set; }
        public DateOnly MenuDate { get; set; }

        /// <summary>Идентификатор сезона, выбранного для меню.</summary>
        public Guid? SeasonId { get; set; }
        public string SeasonName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
