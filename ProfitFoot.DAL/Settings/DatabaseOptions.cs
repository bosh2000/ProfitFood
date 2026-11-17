namespace ProfitFoot.Infrastructure.Settings
{
    /// <summary>
    /// Описывает расположение файла базы данных SQLite на текущей машине.
    /// Значения читаются из секции "Database" файла appsettings.json.
    /// </summary>
    public sealed class DatabaseOptions
    {
        /// <summary>
        /// Имя секции конфигурации, в которой хранятся эти настройки.
        /// </summary>
        public const string Section = "Database";

        /// <summary>
        /// Явный путь к файлу SQLite. Если значение не задано, используется путь текущего пользователя
        /// %LOCALAPPDATA%\ProfitFood\profitfood.db.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Путь, который использовали предыдущие версии программы (жёстко заданный путь на диске).
        /// Резолвер один раз копирует файл отсюда, если целевой файл ещё не существует.
        /// </summary>
        public string? LegacyPath { get; set; }
    }
}
