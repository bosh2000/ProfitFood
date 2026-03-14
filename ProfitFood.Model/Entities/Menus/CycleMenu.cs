using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Цикличное меню.
    /// </summary>
    public sealed class CycleMenu : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Название меню
        public Guid? SeasonId { get; set; } // FK сезона
        public Guid? AgeGroupId { get; set; } // FK возрастной группы
        public int DaysCount { get; set; } // Длительность цикла
        public DateOnly? DateFrom { get; set; } // Начало действия
        public DateOnly? DateTo { get; set; } // Конец действия

        public Season? Season { get; set; } // Сезон
        public AgeGroup? AgeGroup { get; set; } // Возрастная группа
        public ICollection<CycleMenuDay> Days { get; set; } = new List<CycleMenuDay>(); // Дни цикла
        public ICollection<DailyMenu> DailyMenus { get; set; } = new List<DailyMenu>(); // Меню на дату, созданные из шаблона
    }
}