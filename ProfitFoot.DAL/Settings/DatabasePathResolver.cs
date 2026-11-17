namespace ProfitFoot.Infrastructure.Settings
{
    /// <summary>
    /// Вычисляет физический путь к файлу SQLite для текущего пользователя и готовит его каталог.
    /// </summary>
    public static class DatabasePathResolver
    {
        private const string ProductFolderName = "ProfitFood";
        private const string DatabaseFileName = "profitfood.db";

        // SQLite может держать рядом с базой журнал упреждающей записи и файл разделяемой памяти:
        // копирование только основного файла потеряет последние транзакции.
        private static readonly string[] SidecarSuffixes = ["-wal", "-shm"];

        /// <summary>
        /// Определяет абсолютный путь к базе данных, создаёт родительский каталог и при необходимости
        /// один раз переносит файл из прежнего расположения.
        /// </summary>
        /// <param name="options">Настройки расположения базы данных; не должны быть null.</param>
        /// <returns>Абсолютный путь к файлу базы данных SQLite.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="options"/> равно null.</exception>
        public static string Resolve(DatabaseOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var targetPath = string.IsNullOrWhiteSpace(options.Path)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    ProductFolderName,
                    DatabaseFileName)
                : Environment.ExpandEnvironmentVariables(options.Path);

            // LocalApplicationData всегда доступна текущему пользователю на запись, но явный путь
            // из конфигурации может указывать на каталог, которого ещё нет.
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            MigrateLegacyDatabase(options.LegacyPath, targetPath);
            return targetPath;
        }

        /// <summary>
        /// Копирует базу данных из расположения, которое использовали предыдущие версии, если целевой
        /// файл отсутствует. Существующий целевой файл никогда не перезаписывается, поэтому данные
        /// пользователя сохраняются при каждом запуске.
        /// </summary>
        /// <param name="legacyPath">Настроенный прежний путь; может быть null или пустым.</param>
        /// <param name="targetPath">Вычисленный целевой путь.</param>
        private static void MigrateLegacyDatabase(string? legacyPath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(legacyPath) || File.Exists(targetPath))
            {
                return;
            }

            var sourcePath = Environment.ExpandEnvironmentVariables(legacyPath);
            if (!File.Exists(sourcePath))
            {
                return;
            }

            File.Copy(sourcePath, targetPath);

            foreach (var suffix in SidecarSuffixes)
            {
                if (File.Exists(sourcePath + suffix))
                {
                    File.Copy(sourcePath + suffix, targetPath + suffix, overwrite: true);
                }
            }
        }
    }
}
