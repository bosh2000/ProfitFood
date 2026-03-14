using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Базовая ViewModel с уведомлением об изменении свойств.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
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