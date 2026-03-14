using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Recipes
{
    /// <summary>
    /// Технологическая карта блюда.
    /// </summary>
    public sealed class RecipeCard : ReferenceEntityBase
    {
        public Guid DishId { get; set; } // FK блюда
        public string CardNumber { get; set; } = null!; // Номер техкарты
        public string Name { get; set; } = null!; // Наименование техкарты

        public Dish Dish { get; set; } = null!; // Блюдо
        public ICollection<RecipeCardVersion> Versions { get; set; } = new List<RecipeCardVersion>(); // Версии техкарты
    }
}