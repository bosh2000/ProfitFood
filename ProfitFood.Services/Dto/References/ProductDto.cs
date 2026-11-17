namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника продуктов питания.</summary>
    public sealed class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Article { get; set; }
        public Guid ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; } = string.Empty;
        public Guid BaseUnitId { get; set; }
        public string BaseUnitName { get; set; } = string.Empty;
        public Guid? StorageUnitId { get; set; }
        public string StorageUnitName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPerishable { get; set; }
        public decimal MinStock { get; set; }
    }
}
