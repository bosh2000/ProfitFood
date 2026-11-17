using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.IO;

namespace ProfitFood.UI.Logging
{
    /// <summary>
    /// Собирает файловый логгер приложения. У десктопного приложения нет консоли,
    /// поэтому единственное надёжное хранилище — ротируемый файл в папке данных пользователя.
    /// </summary>
    public static class LoggingSetup
    {
        private const string ProductFolderName = "ProfitFood";
        private const string LogFolderName = "logs";
        private const string LogFileName = "app-.log";
        private const int RetainedFileCountLimit = 14;

        /// <summary>
        /// Каталог, в который пишутся файлы журнала: %LOCALAPPDATA%\ProfitFood\logs.
        /// </summary>
        public static string LogDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProductFolderName,
            LogFolderName);

        /// <summary>
        /// Создаёт логгер с файловым приёмником и суточной ротацией; уровень берётся из конфигурации.
        /// </summary>
        /// <param name="appConfiguration">
        /// Конфигурация приложения; читается ключ "Logging:MinimumLevel" (по умолчанию Information).
        /// </param>
        /// <returns>Настроенный логгер Serilog.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="appConfiguration"/> равно null.</exception>
        public static Serilog.ILogger CreateLogger(IConfiguration appConfiguration)
        {
            ArgumentNullException.ThrowIfNull(appConfiguration);

            Directory.CreateDirectory(LogDirectory);

            return new LoggerConfiguration()
                .MinimumLevel.Is(ReadMinimumLevel(appConfiguration))
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(LogDirectory, LogFileName),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: RetainedFileCountLimit,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// Читает минимальный уровень логирования из конфигурации.
        /// </summary>
        /// <param name="appConfiguration">Конфигурация приложения.</param>
        /// <returns>
        /// Разобранный уровень либо <see cref="LogEventLevel.Information"/>, если значение отсутствует
        /// или задано с ошибкой: опечатка в файле настроек не должна мешать запуску приложения.
        /// </returns>
        private static LogEventLevel ReadMinimumLevel(IConfiguration appConfiguration)
        {
            var configured = appConfiguration["Logging:MinimumLevel"];

            return Enum.TryParse<LogEventLevel>(configured, ignoreCase: true, out var level)
                ? level
                : LogEventLevel.Information;
        }
    }
}