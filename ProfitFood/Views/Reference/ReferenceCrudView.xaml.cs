using System.Windows.Controls;

namespace ProfitFood.UI.Views.Reference
{
    /// <summary>
    /// Общее представление CRUD-справочника: таблица записей и карточка редактирования.
    /// Используется всеми простыми справочниками одного типа.
    /// </summary>
    public partial class ReferenceCrudView : UserControl
    {
        /// <summary>
        /// Инициализирует представление.
        /// </summary>
        public ReferenceCrudView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Скрывает служебные столбцы идентификаторов: таблица строится автоматически по свойствам DTO,
        /// и колонки вида Id и AgeGroupId пользователю не нужны.
        /// </summary>
        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var propertyName = e.PropertyName;

            if (propertyName == "Id" || propertyName.EndsWith("Id", StringComparison.Ordinal))
            {
                e.Cancel = true;
            }
        }
    }
}
