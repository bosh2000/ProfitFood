namespace ProfitFood.Applications.Dto.Menus
{
    /// <summary>Строка меню-требования: сколько продукта передать на кухню.</summary>
    public sealed class MenuRequirementItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public int SortOrder { get; set; }
    }
}
