namespace ProfitFood.Applications.Dto.Recipes
{
    /// <summary>Выход блюда для конкретной возрастной группы.</summary>
    public sealed class DishOutputDto
    {
        public Guid Id { get; set; }

        /// <summary>Идентификатор возрастной группы.</summary>
        public Guid AgeGroupId { get; set; }

        /// <summary>Наименование возрастной группы.</summary>
        public string AgeGroupName { get; set; } = string.Empty;

        /// <summary>Нормативный выход порции для этой группы.</summary>
        public decimal OutputQuantity { get; set; }

        public Guid UnitId { get; set; }

        /// <summary>Наименование единицы измерения выхода.</summary>
        public string UnitName { get; set; } = string.Empty;
    }
}
