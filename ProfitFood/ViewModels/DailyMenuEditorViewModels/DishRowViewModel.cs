namespace ProfitFood.UI.ViewModels.DailyMenuEditorViewModels
{
    /// <summary>
    /// Строка блюда в меню.
    /// </summary>
    public sealed class DishRowViewModel : ViewModelBase
    {
        private int _sortOrder;
        private string _dishName;
        private string _notes;

        public DishRowViewModel(string mealTypeName, int sortOrder, string dishName, string recipeCardNumber, string outputText, string notes)
        {
            MealTypeName = mealTypeName;
            _sortOrder = sortOrder;
            _dishName = dishName;
            RecipeCardNumber = recipeCardNumber;
            OutputText = outputText;
            _notes = notes;
        }

        public string MealTypeName { get; }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetProperty(ref _sortOrder, value);
        }

        public string DishName
        {
            get => _dishName;
            set => SetProperty(ref _dishName, value);
        }

        public string RecipeCardNumber { get; }
        public string OutputText { get; }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
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