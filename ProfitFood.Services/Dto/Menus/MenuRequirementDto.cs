using ProfitFood.Domain.Entities.Enums;

namespace ProfitFood.Applications.Dto.Menus
{
    /// <summary>Меню-требование на выдачу продуктов со склада.</summary>
    public sealed class MenuRequirementDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateOnly RequirementDate { get; set; }
        public Guid DailyMenuId { get; set; }
        public Guid StorageLocationId { get; set; }
        public string StorageLocationName { get; set; } = string.Empty;
        public MenuRequirementStatus Status { get; set; }

        /// <summary>Отображаемое имя статуса.</summary>
        public string StatusName => Status.ToString();

        /// <summary>Общее количество детей, на которое рассчитано требование.</summary>
        public int TotalChildrenCount { get; set; }

        public string? Notes { get; set; }

        /// <summary>Строки документа.</summary>
        public List<MenuRequirementItemDto> Items { get; set; } = new();
    }
}
