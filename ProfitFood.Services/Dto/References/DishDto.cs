namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Блюдо для справочника и выбора в техкартах.</summary>
    public sealed class DishDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid DishCategoryId { get; set; }
        public string DishCategoryName { get; set; } = string.Empty;
        public Guid MealTypeId { get; set; }
        public string MealTypeName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
