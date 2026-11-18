namespace ProfitFood.Applications.Dto.Recipes
{
    /// <summary>Строка списка технологических карт.</summary>
    public sealed class RecipeCardListItemDto
    {
        public Guid Id { get; set; }

        /// <summary>Блюдо, к которому относится техкарта.</summary>
        public Guid DishId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? VersionNumber { get; set; }
        public DateOnly? VersionDateFrom { get; set; }
        public decimal? OutputQuantity { get; set; }
        public string OutputUnitName { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }
}
