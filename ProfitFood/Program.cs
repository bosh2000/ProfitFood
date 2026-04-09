using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.DAL.Repository.Implementation;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.Mappings;
using ProfitFood.UI.ViewModels;
using ProfitFood.UI.ViewModels.Reference;
using ProfitFood.UI.ViewModels.Reference.Units;
using ProfitFoot.Infrastructure;
using ProfitFood.Applications.Services.References;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Repository.Implementation;

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
                        option => option.UseSqlite("Data Source=D:\\DbProfitFood\\profitfood.db")
                        );
                    services.AddAutoMapper(typeof(UnitMappingProfile));
                    services.AddScoped<IUnitAppService, UnitAppService>();
                    services.AddScoped<IDbRepository, DbRepository>();
                    services.AddScoped<MainWindow>();
                    services.AddScoped<MainWindowViewModel>();
                    services.AddScoped<UnitsReferenceViewModel>();
                    services.AddScoped<ReferencesViewModel>();

                    //   services.AddScoped<ProductTabViewModel>();
                    //   services.AddScoped<BaseUnitTabViewModel>();

                    services.AddScoped<App>();
                }).
                Build();
            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<App>();
            app?.Run();
        }
    }
}