using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Domain.Entities
{
    /// <summary>
    /// Глобальные настройки приложения и учреждения.
    /// </summary>
    public sealed class AppSettings : EntityBase
    {
        public string InstitutionName { get; set; } = null!; // Полное наименование учреждения
        public string InstitutionShortName { get; set; } = null!; // Краткое наименование
        public string Address { get; set; } = null!; // Адрес
        public string Phone { get; set; } = null!; // Телефон
        public Guid? DefaultStorageLocationId { get; set; } // FK склада по умолчанию
        public string? Form299Title { get; set; } // Заголовок формы 299
        public string? DirectorName { get; set; } // Руководитель учреждения

        public StorageLocation? DefaultStorageLocation { get; set; } // Склад по умолчанию
    }
}