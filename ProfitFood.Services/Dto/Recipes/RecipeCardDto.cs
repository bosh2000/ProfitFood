namespace ProfitFood.Applications.Dto.Recipes
{
    /// <summary>Технологическая карта вместе с редактируемой версией.</summary>
    public sealed class RecipeCardDto
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid DishId { get; set; }
        public string DishName { get; set; } = string.Empty;

        /// <summary>Редактируемая версия техкарты.</summary>
        public RecipeCardVersionDto Version { get; set; } = new();
    }
}
