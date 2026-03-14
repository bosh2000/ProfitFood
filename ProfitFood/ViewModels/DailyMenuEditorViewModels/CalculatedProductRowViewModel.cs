namespace ProfitFood.UI.ViewModels.DailyMenuEditorViewModels
{
    /// <summary>
    /// Строка рассчитанного продукта.
    /// </summary>
    public sealed class CalculatedProductRowViewModel : ViewModelBase
    {
        private decimal _requiredQuantity;
        private decimal _stockQuantity;
        private decimal _deficitQuantity;
        private string _statusComment;

        public CalculatedProductRowViewModel(string productName, string unitShortName, decimal requiredQuantity, decimal stockQuantity, decimal deficitQuantity, string statusComment)
        {
            ProductName = productName;
            UnitShortName = unitShortName;
            _requiredQuantity = requiredQuantity;
            _stockQuantity = stockQuantity;
            _deficitQuantity = deficitQuantity;
            _statusComment = statusComment;
        }

        public string ProductName { get; }
        public string UnitShortName { get; }

        public decimal RequiredQuantity
        {
            get => _requiredQuantity;
            set
            {
                if (SetProperty(ref _requiredQuantity, value))
                {
                    OnPropertyChanged(nameof(RequiredQuantityText));
                }
            }
        }

        public decimal StockQuantity
        {
            get => _stockQuantity;
            set
            {
                if (SetProperty(ref _stockQuantity, value))
                {
                    OnPropertyChanged(nameof(StockQuantityText));
                }
            }
        }

        public decimal DeficitQuantity
        {
            get => _deficitQuantity;
            set
            {
                if (SetProperty(ref _deficitQuantity, value))
                {
                    OnPropertyChanged(nameof(DeficitQuantityText));
                }
            }
        }

        public string StatusComment
        {
            get => _statusComment;
            set => SetProperty(ref _statusComment, value);
        }

        public string RequiredQuantityText => RequiredQuantity.ToString("0.###");
        public string StockQuantityText => StockQuantity.ToString("0.###");
        public string DeficitQuantityText => DeficitQuantity.ToString("0.###");
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