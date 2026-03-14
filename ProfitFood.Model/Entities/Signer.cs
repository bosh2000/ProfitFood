using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Domain.Entities
{
    /// <summary>
    /// Подписант документов.
    /// </summary>
    public sealed class Signer : ReferenceEntityBase
    {
        public string FullName { get; set; } = null!; // ФИО
        public string Position { get; set; } = null!; // Должность

        public ICollection<MenuRequirement> HeadSignedRequirements { get; set; } = new List<MenuRequirement>(); // Требования, где подписант руководитель
        public ICollection<MenuRequirement> StorekeeperSignedRequirements { get; set; } = new List<MenuRequirement>(); // Требования, где подписант кладовщик
        public ICollection<MenuRequirement> MedWorkerSignedRequirements { get; set; } = new List<MenuRequirement>(); // Требования, где подписант медработник
    }
}
