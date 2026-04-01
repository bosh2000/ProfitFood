namespace ProfitFood.UI.ViewModels.DashBoardViewModels
{
    /// <summary>
    /// Строка критического остатка.
    /// </summary>
    public sealed class CriticalStockRowViewModel
    {
        public CriticalStockRowViewModel(string productName, string stockText, string minStockText)
        {
            ProductName = productName;
            StockText = stockText;
            MinStockText = minStockText;
        }

        public string ProductName { get; }
        public string StockText { get; }
        public string MinStockText { get; }
    }
}