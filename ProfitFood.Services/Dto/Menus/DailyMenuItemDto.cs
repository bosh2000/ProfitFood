namespace ProfitFood.Applications.Dto.Menus
{
    /// <summary>Позиция ежедневного меню для отображения в сводках.</summary>
    public sealed class DailyMenuItemDto
    {
        /// <summary>Наименование приёма пищи.</summary>
        public string MealTypeName { get; set; } = string.Empty;

        /// <summary>Наименование блюда.</summary>
        public string DishName { get; set; } = string.Empty;

        /// <summary>Номер технологической карты, по которой готовится блюдо.</summary>
        public string RecipeCardNumber { get; set; } = string.Empty;

        /// <summary>Нормативный выход порции.</summary>
        public decimal? OutputQuantity { get; set; }

        /// <summary>Единица измерения выхода.</summary>
        public string OutputUnitName { get; set; } = string.Empty;
    }
}
