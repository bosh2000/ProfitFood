namespace ProfitFood.Domain.Entities
{
    /// <summary>
    /// Журнал изменений сущностей.
    /// </summary>
    public sealed class AuditLog : EntityBase
    {
        public string EntityName { get; set; } = null!; // Имя сущности
        public Guid EntityId { get; set; } // Идентификатор сущности
        public string ActionType { get; set; } = null!; // Тип действия
        public DateTime ChangedAt { get; set; } // Время изменения
        public string OldValueJson { get; set; } = null!; // Старое значение
        public string NewValueJson { get; set; } = null!; // Новое значение
    }
}