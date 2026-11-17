namespace ProfitFood.Applications.Dto.Documents
{
    /// <summary>Строка складского документа для формы заполнения.</summary>
    public sealed class StockDocumentItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }
}
