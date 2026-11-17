namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника категорий блюд.</summary>
    public sealed class DishCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
