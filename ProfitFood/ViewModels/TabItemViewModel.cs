using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    /// <summary>
    /// Вкладка рабочей области: раздел приложения или открытый документ.
    /// </summary>
    public sealed class TabItemViewModel : ViewModelBase
    {
        private string _header;

        /// <summary>
        /// Инициализирует вкладку.
        /// </summary>
        /// <param name="key">
        /// Ключ вкладки: для разделов используется тип ViewModel, для документов — уникальный идентификатор,
        /// чтобы можно было открыть несколько документов одного типа.
        /// </param>
        /// <param name="header">Заголовок на ярлыке вкладки.</param>
        /// <param name="content">ViewModel, отображаемая в содержимом вкладки.</param>
        /// <param name="closeCommand">Действие, вызываемое командой закрытия вкладки.</param>
        /// <exception cref="ArgumentNullException">Возникает, если обязательные аргументы пусты.</exception>
        public TabItemViewModel(object key, string header, object content, Action<TabItemViewModel> closeCommand)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            _header = header ?? string.Empty;
            Content = content ?? throw new ArgumentNullException(nameof(content));

            ArgumentNullException.ThrowIfNull(closeCommand);

            // Команда передаёт саму вкладку: закрытие выполняет владелец коллекции, а не сама вкладка.
            CloseCommand = new RelayCommand(_ => closeCommand(this));
        }

        /// <summary>
        /// Ключ вкладки, по которому раздел находит уже открытую вкладку.
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// Заголовок на ярлыке вкладки.
        /// </summary>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// ViewModel, отображаемая в содержимом вкладки.
        /// </summary>
        public object Content { get; }

        /// <summary>
        /// Команда закрытия вкладки.
        /// </summary>
        public ICommand CloseCommand { get; }
    }
}
