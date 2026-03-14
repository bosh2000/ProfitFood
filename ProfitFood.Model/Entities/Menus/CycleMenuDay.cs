namespace ProfitFood.Domain.Entities.Menus
{
    /// <summary>
    /// День цикличного меню.
    /// </summary>
    public sealed class CycleMenuDay : EntityBase
    {
        public Guid CycleMenuId { get; set; } // FK цикличного меню
        public int DayNumber { get; set; } // Номер дня
        public string? Name { get; set; } // Наименование дня

        public CycleMenu CycleMenu { get; set; } = null!; // Цикличное меню
        public ICollection<CycleMenuItem> Items { get; set; } = new List<CycleMenuItem>(); // Позиции дня
    }
}