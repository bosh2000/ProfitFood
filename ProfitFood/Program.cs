using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProfitFood.DAL.Repository.Implementation;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.Mappings;
using ProfitFood.UI.ViewModels;
using ProfitFood.UI.ViewModels.BaseUnitViewModels;
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
                    services.AddAutoMapper(typeof(AutoMapperProfile));
                    services.AddScoped<IProfitDbRepository, ProfitDbRepository>();
                    services.AddScoped<MainWindow>();
                    services.AddScoped<MainWindowViewModel>();
                    services.AddScoped<ProductTabViewModel>();
                    services.AddScoped<BaseUnitTabViewModel>();

                    services.AddScoped<App>();
                }).
                Build();
            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<App>();
            app?.Run();
        }
    }
}