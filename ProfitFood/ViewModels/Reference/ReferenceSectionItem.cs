namespace ProfitFood.UI.ViewModels.Reference
{
    /// <summary>
    /// Пункт списка справочников: подпись и тип ViewModel, которая открывается справа.
    /// </summary>
    public sealed class ReferenceSectionItem
    {
        /// <summary>
        /// Инициализирует пункт списка справочников.
        /// </summary>
        /// <param name="title">Подпись в списке слева.</param>
        /// <param name="viewModelType">Тип ViewModel справочника; разрешается через DI при открытии.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public ReferenceSectionItem(string title, Type viewModelType)
        {
            Title = string.IsNullOrWhiteSpace(title)
                ? throw new ArgumentException("Подпись справочника не может быть пустой.", nameof(title))
                : title;

            ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
        }

        /// <summary>
        /// Подпись в списке слева.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Тип ViewModel, открываемой при выборе пункта.
        /// </summary>
        public Type ViewModelType { get; }
    }
}
