using Microsoft.Extensions.Logging;
using ProfitFood.UI.Logging;
using System.Windows;
using System.Windows.Threading;

namespace ProfitFood.UI
{
    /// <summary>
    /// Объект WPF-приложения. Подключает глобальные обработчики ошибок, чтобы ни один сбой
    /// ни в одном потоке не завершил процесс молча.
    /// </summary>
    public class App : Application
    {
        private readonly MainWindow _mainWindow;
        private readonly ILogger<App> _logger;

        /// <summary>
        /// Инициализирует объект приложения.
        /// </summary>
        /// <param name="mainWindow">Главное окно, показываемое при запуске.</param>
        /// <param name="logger">Логгер, используемый глобальными обработчиками ошибок.</param>
        public App(MainWindow mainWindow, ILogger<App> logger)
        {
            _mainWindow = mainWindow;
            _logger = logger;
        }

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            // Подключаются до появления первого окна: сбой при построении оболочки должен быть
            // обработан так же, как и любой последующий.
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            _mainWindow.Show();
            base.OnStartup(e);
        }

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            // Отписка не даёт обработчикам сработать по уже завершающимся объектам.
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException -= OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

            base.OnExit(e);
        }

        /// <summary>
        /// Обрабатывает исключения в потоке пользовательского интерфейса. Приложение продолжает
        /// работу, чтобы одна неудачная команда не уничтожила несохранённые данные пользователя.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            _logger.LogError(args.Exception, "Необработанное исключение в потоке пользовательского интерфейса");

            MessageBox.Show(
                $"{args.Exception.Message}\n\nПодробности записаны в журнал:\n{LoggingSetup.LogDirectory}",
                "Непредвиденная ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            args.Handled = true;
        }

        /// <summary>
        /// Обрабатывает исключения, прошедшие мимо всех блоков catch. Процесс уже завершается,
        /// поэтому остаётся только зафиксировать причину.
        /// </summary>
        private void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs args)
        {
            _logger.LogCritical(
                args.ExceptionObject as Exception,
                "Фатальное необработанное исключение, процесс завершается: {IsTerminating}",
                args.IsTerminating);
        }

        /// <summary>
        /// Обрабатывает исключения задач, которые никто не ожидал через await; без этого хука они
        /// всплывают только при финализации, далеко от места возникновения.
        /// </summary>
        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
        {
            _logger.LogError(args.Exception, "Необработанное исключение задачи");
            args.SetObserved();
        }
    }
}
