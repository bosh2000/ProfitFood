using ProfitFood.Domain.Entities.Enums;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// Ежедневное меню на конкретную дату.
    /// </summary>
    public sealed class DailyMenu : EntityBase
    {
        public DateOnly MenuDate { get; set; } // Дата меню
        public Guid? SeasonId { get; set; } // FK сезона
        public Guid? CycleMenuId { get; set; } // FK шаблона, если заполнено из цикличного меню
        public DailyMenuStatus Status { get; set; } // Статус меню
        public string? Notes { get; set; } // Примечание
        public DateTime CreatedAt { get; set; } // Дата создания
        public DateTime UpdatedAt { get; set; } // Дата изменения

        public Season? Season { get; set; } // Сезон
        public CycleMenu? CycleMenu { get; set; } // Шаблон-источник
        public ICollection<DailyMenuAttendance> Attendances { get; set; } = new List<DailyMenuAttendance>(); // Посещаемость
        public ICollection<DailyMenuItem> Items { get; set; } = new List<DailyMenuItem>(); // Блюда меню
        public MenuRequirement? MenuRequirement { get; set; } // Связанное меню-требование
    }
}