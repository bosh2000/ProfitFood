namespace ProfitFood.Applications.Dto.Stocks
{
    /// <summary>Строка остатка продукта на складе.</summary>
    public sealed class StockBalanceDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>Идентификатор продукта для карточки редактирования.</summary>
        public Guid ProductId { get; set; }

        /// <summary>Идентификатор склада для карточки редактирования.</summary>
        public Guid StorageLocationId { get; set; }

        /// <summary>Идентификатор единицы измерения для карточки редактирования.</summary>
        public Guid UnitId { get; set; }
        public string StorageLocationName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal MinStock { get; set; }

        /// <summary>Признак того, что остаток ниже минимального.</summary>
        public bool IsDeficit { get; set; }
    }
}
