namespace ProfitFood.Applications.Dto.Recipes
{
    /// <summary>Версия технологической карты с ингредиентами.</summary>
    public sealed class RecipeCardVersionDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public decimal OutputQuantity { get; set; }
        public Guid OutputUnitId { get; set; }
        public string OutputUnitName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsApproved { get; set; }

        /// <summary>Ингредиенты версии.</summary>
        public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    }
}
