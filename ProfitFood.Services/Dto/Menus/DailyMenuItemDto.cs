namespace ProfitFood.Applications.Dto.Menus
{
    /// <summary>Позиция ежедневного меню: блюдо, приём пищи и версия техкарты.</summary>
    public sealed class DailyMenuItemDto
    {
        /// <summary>Идентификатор строки; пустой у новой позиции.</summary>
        public Guid Id { get; set; }

        /// <summary>Приём пищи.</summary>
        public Guid MealTypeId { get; set; }

        /// <summary>Наименование приёма пищи.</summary>
        public string MealTypeName { get; set; } = string.Empty;

        /// <summary>Блюдо.</summary>
        public Guid DishId { get; set; }

        /// <summary>Наименование блюда.</summary>
        public string DishName { get; set; } = string.Empty;

        /// <summary>Версия технологической карты, по которой готовится блюдо.</summary>
        public Guid RecipeCardVersionId { get; set; }

        /// <summary>Номер технологической карты.</summary>
        public string RecipeCardNumber { get; set; } = string.Empty;

        /// <summary>Нормативный выход порции.</summary>
        public decimal? OutputQuantity { get; set; }

        /// <summary>Единица измерения выхода.</summary>
        public string OutputUnitName { get; set; } = string.Empty;

        /// <summary>Порядок вывода внутри приёма пищи.</summary>
        public int SortOrder { get; set; }
    }
}
