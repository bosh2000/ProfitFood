namespace ProfitFood.UI.ViewModels.DashBoardViewModels
{
    /// <summary>
    /// Строка последнего документа.
    /// </summary>
    public sealed class LastDocumentRowViewModel
    {
        public LastDocumentRowViewModel(string documentName, string dateText, string statusName)
        {
            DocumentName = documentName;
            DateText = dateText;
            StatusName = statusName;
        }

        public string DocumentName { get; }
        public string DateText { get; }
        public string StatusName { get; }
    }
}