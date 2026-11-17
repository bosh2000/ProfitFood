namespace ProfitFood.Applications.Dto.Recipes
{
    /// <summary>Ингредиент версии техкарты.</summary>
    public sealed class RecipeIngredientDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal GrossQuantity { get; set; }
        public decimal NetQuantity { get; set; }

        /// <summary>Процент потерь; пока вводится вручную и не пересчитывает нетто.</summary>
        public decimal LossPercent { get; set; }

        public int SortOrder { get; set; }

        /// <summary>Сумма нетто, справочная величина для проверки заполнения.</summary>
        public decimal Amount => NetQuantity;
    }
}
