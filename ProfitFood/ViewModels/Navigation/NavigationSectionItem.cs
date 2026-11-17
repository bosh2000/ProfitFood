namespace ProfitFood.UI.ViewModels.Navigation
{
    /// <summary>
    /// Пункт бокового меню: раздел приложения и тип ViewModel, которая его отображает.
    /// </summary>
    public sealed class NavigationSectionItem
    {
        /// <summary>
        /// Инициализирует пункт меню.
        /// </summary>
        /// <param name="title">Подпись пункта в боковом меню.</param>
        /// <param name="viewModelType">Тип ViewModel раздела; разрешается через DI при открытии.</param>
        /// <exception cref="ArgumentNullException">Возникает, если аргументы пусты.</exception>
        public NavigationSectionItem(string title, Type viewModelType)
        {
            Title = string.IsNullOrWhiteSpace(title)
                ? throw new ArgumentException("Подпись раздела не может быть пустой.", nameof(title))
                : title;

            ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
        }

        /// <summary>
        /// Подпись пункта в боковом меню.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Тип ViewModel, которая открывается при выборе раздела.
        /// </summary>
        public Type ViewModelType { get; }
    }
}
