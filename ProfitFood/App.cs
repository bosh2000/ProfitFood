using ProfitFood.DAL.Repository.Interfaces;
using System.Windows;

namespace ProfitFood.UI
{
    public class App : Application
    {
        private readonly MainWindow _mainWindow;
        private readonly IProfitDbRepository _repository;

        public App(MainWindow mainWindow, IProfitDbRepository repository)
        {
            this._mainWindow = mainWindow;
            this._repository = repository;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _mainWindow.Show();
            base.OnStartup(e);
        }
    }
}