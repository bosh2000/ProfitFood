namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника типов приёмов пищи.</summary>
    public sealed class MealTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
