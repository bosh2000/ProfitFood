namespace ProfitFood.UI.ViewModels.DailyMenuEditorViewModels
{
    /// <summary>
    /// Строка связанного документа.
    /// </summary>
    public sealed class RelatedDocumentRowViewModel
    {
        public RelatedDocumentRowViewModel(string documentTypeName, string number, string dateText, string statusName, string comment)
        {
            DocumentTypeName = documentTypeName;
            Number = number;
            DateText = dateText;
            StatusName = statusName;
            Comment = comment;
        }

        public string DocumentTypeName { get; }
        public string Number { get; }
        public string DateText { get; }
        public string StatusName { get; }
        public string Comment { get; }
    }
}

/*
Пример подключения дизайн-данных в XAML:

<UserControl ...
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:ProfitFood.UI.ViewModels"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:DailyMenuEditorViewModel, IsDesignTimeCreatable=True}">

Если нужен runtime DataContext без DI:

public partial class DailyMenuEditorView : UserControl
{
    public DailyMenuEditorView()
    {
        InitializeComponent();
        DataContext = new DailyMenuEditorViewModel();
    }
}
*/