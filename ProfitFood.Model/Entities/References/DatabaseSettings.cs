namespace ProfitFood.Domain.Entities
{
    /// <summary>
    /// Настройки подключения и поведения БД.
    /// Обычно хранится одна запись.
    /// </summary>
    public sealed class DatabaseSettings : EntityBase
    {
        public string ProviderName { get; set; } = null!; // Провайдер БД, например SQLite
        public string DatabaseFilePath { get; set; } = null!; // Путь к файлу БД
        public bool AutoMigrateOnStartup { get; set; } // Выполнять миграции при старте
        public bool CreateBackupBeforeMigration { get; set; } // Создавать резервную копию перед миграцией
        public DateTime? LastBackupAt { get; set; } // Дата последней резервной копии
        public string? Notes { get; set; } // Комментарий к настройкам
    }
}
