namespace ProfitFood.Applications.Dto.References
{
    /// <summary>Строка справочника мест хранения продуктов.</summary>
    public sealed class StorageLocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
