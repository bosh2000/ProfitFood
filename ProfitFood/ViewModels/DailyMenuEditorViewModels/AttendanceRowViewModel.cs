namespace ProfitFood.UI.ViewModels.DailyMenuEditorViewModels
{
    /// <summary>
    /// Строка посещаемости по группе детей.
    /// </summary>
    public sealed class AttendanceRowViewModel : ViewModelBase
    {
        private int _actualCount;
        private string _comment;

        public AttendanceRowViewModel(string childGroupName, string ageGroupName, int plannedCount, int actualCount, string comment)
        {
            ChildGroupName = childGroupName;
            AgeGroupName = ageGroupName;
            PlannedCount = plannedCount;
            _actualCount = actualCount;
            _comment = comment;
        }

        public string ChildGroupName { get; }
        public string AgeGroupName { get; }
        public int PlannedCount { get; }

        public int ActualCount
        {
            get => _actualCount;
            set => SetProperty(ref _actualCount, value);
        }

        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }
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