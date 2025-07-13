using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProfitFood.DAL.Repository.Implementation;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.ViewModels;
using ProfitFoot.DAL;

namespace ProfitFood.UI
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ProfitFoodDbContext>(
                        option => option.UseSqlite("Data Source=profitfood.db")
                        );
                    services.AddScoped<IProfitDbRepository, ProfitDbRepository>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainWindowViewModel>();

                    services.AddSingleton<App>();
                }).
                Build();
            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<App>();
            app?.Run();
        }
    }
}