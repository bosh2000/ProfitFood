namespace ProfitFood.UI.ViewModels.DailyMenuEditorViewModels
{
    /// <summary>
    /// Сезон для ComboBox.
    /// </summary>
    public sealed class SeasonItemViewModel
    {
        public SeasonItemViewModel(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
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