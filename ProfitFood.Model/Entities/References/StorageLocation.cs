using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Место хранения продуктов.
    /// </summary>
    public sealed class StorageLocation : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Наименование склада
        public string Code { get; set; } = null!; // Код склада

        public ICollection<StockDocument> StockDocuments { get; set; } = new List<StockDocument>(); // Документы по складу
        public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>(); // Остатки по складу
        public ICollection<MenuRequirement> MenuRequirements { get; set; } = new List<MenuRequirement>(); // Требования со склада
        public ICollection<AppSettings> SettingsAsDefaultStorage { get; set; } = new List<AppSettings>(); // Настройки, где склад по умолчанию
    }
}