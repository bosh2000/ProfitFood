namespace ProfitFood.Applications.Dto.Documents
{
    /// <summary>Строка журнала складских документов.</summary>
    public sealed class StockDocumentListItemDto
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateOnly DocumentDate { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string StorageLocationName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
    }
}
