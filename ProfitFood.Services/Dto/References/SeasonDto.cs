namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника сезонов меню.</summary>
    public sealed class SeasonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MonthFrom { get; set; }
        public int MonthTo { get; set; }
    }
}
