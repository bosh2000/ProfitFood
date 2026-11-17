using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.Applications.Services.References;
using ProfitFood.DAL.Repository.Implementation;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Infrastructure.Repository.Implementation;
using ProfitFood.Infrastructure.Repository.Interfaces;
using ProfitFood.Infrastructure.Services.References;
using ProfitFood.Infrastructure.Services.Documents;
using ProfitFood.Infrastructure.Services.Menus;
using ProfitFood.Infrastructure.Services.Recipes;
using ProfitFood.Infrastructure.Services.Stocks;
using ProfitFood.UI.Logging;
using ProfitFood.UI.Mappings;
using ProfitFood.UI.ViewModels;
using ProfitFood.UI.ViewModels.DailyMenuEditorViewModels;
using ProfitFood.UI.ViewModels.DashBoardViewModels;
using ProfitFood.UI.ViewModels.Reference;
using ProfitFood.UI.ViewModels.Reference.AgeGroups;
using ProfitFood.UI.ViewModels.Reference.ChildGroups;
using ProfitFood.UI.ViewModels.Reference.DishCategories;
using ProfitFood.UI.ViewModels.Reference.Dishes;
using ProfitFood.UI.ViewModels.Reference.MealTypes;
using ProfitFood.UI.ViewModels.Reference.ProductCategories;
using ProfitFood.UI.ViewModels.Reference.Products;
using ProfitFood.UI.ViewModels.Reference.Seasons;
using ProfitFood.UI.ViewModels.Reference.StorageLocations;
using ProfitFood.UI.ViewModels.Reference.Units;
using ProfitFood.UI.Views.Reference;
using ProfitFoot.Infrastructure;
using ProfitFoot.Infrastructure.Settings;
using Serilog;

namespace ProfitFood.UI
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            // Хост создаётся позже, но конфигурация и логгер должны существовать уже сейчас:
            // иначе падение при конструировании хоста не оставит следа на диске.
            var bootstrapConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            Log.Logger = LoggingSetup.CreateLogger(bootstrapConfiguration);

            try
            {
                var host = Host.CreateDefaultBuilder()
                    // Корень содержимого по умолчанию — текущий каталог процесса, а он разный
                    // при запуске из Проводника, Visual Studio и установщика; фиксируем его
                    // на папку exe, чтобы appsettings.json всегда находился.
                    .UseContentRoot(AppContext.BaseDirectory)
                    .ConfigureServices((context, services) =>
                    {
                        // Логи фреймворка и EF Core идут через ILogger, а не через статический Log,
                        // поэтому провайдер Serilog регистрируется отдельно.
                        services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: false));

                        var databaseOptions = new DatabaseOptions
                        {
                            Path = context.Configuration[$"{DatabaseOptions.Section}:Path"],
                            LegacyPath = context.Configuration[$"{DatabaseOptions.Section}:LegacyPath"]
                        };

                        // Определяет доступное для записи расположение в профиле пользователя
                        // и один раз переносит базу данных из прежнего места.
                        var databasePath = DatabasePathResolver.Resolve(databaseOptions);
                        Log.Information("Файл базы данных: {DatabasePath}", databasePath);

                        // Регистрируется фабрика, а не контекст: репозитории открывают короткоживущий
                        // контекст на операцию, поэтому долгоживущий трекер не держит сущности в памяти.
                        services.AddDbContextFactory<ProfitFoodDbContext>(option =>
                        {
                            option.UseSqlite($"Data Source={databasePath}");
#if DEBUG
                            // Параметры SQL полезны при отладке и не должны попадать в журнал релизной сборки.
                            option.EnableSensitiveDataLogging();
#endif
                        });
                        services.AddAutoMapper(typeof(UnitMappingProfile));

                        // WPF-приложение, его окна и модели представления живут до завершения процесса,
                        // поэтому они синглтоны; "scoped" лишь имитировал бы более короткое время жизни.
                        services.AddSingleton<IUnitAppService, UnitAppService>();
                        services.AddSingleton<IProductCategoryAppService, ProductCategoryAppService>();

                        // Сервисы остальных справочников: каждый работает со своим репозиторием
                        // и пишет ошибки в общий журнал приложения.
                        services.AddSingleton<IAgeGroupAppService, AgeGroupAppService>();
                        services.AddSingleton<IChildGroupAppService, ChildGroupAppService>();
                        services.AddSingleton<IDishCategoryAppService, DishCategoryAppService>();

                        // Справочник блюд нужен и своему разделу, и техкартам: блюдо — обязательная ссылка техкарты.
                        services.AddSingleton<IDishAppService, DishAppService>();
                        services.AddSingleton<IMealTypeAppService, MealTypeAppService>();
                        services.AddSingleton<IProductAppService, ProductAppService>();
                        services.AddSingleton<ISeasonAppService, SeasonAppService>();
                        services.AddSingleton<IStorageLocationAppService, StorageLocationAppService>();

                        // Сервисы разделов: меню, техкарты, склад и журнал документов.
                        services.AddSingleton<IDailyMenuAppService, DailyMenuAppService>();
                        services.AddSingleton<IRecipeCardAppService, RecipeCardAppService>();
                        services.AddSingleton<IStockAppService, StockAppService>();
                        services.AddSingleton<IDocumentAppService, DocumentAppService>();
                        services.AddSingleton<IDbRepository, DbRepository>();
                        services.AddSingleton<MainWindow>();
                        services.AddSingleton<MainWindowViewModel>();
                        services.AddSingleton<UnitsReferenceViewModel>();
                        services.AddSingleton<ProductCategoriesViewModel>();
                        services.AddSingleton<ReferencesViewModel>();

                        // Разделы живут всё время работы приложения, а редактор меню открывается
                        // отдельным документом, поэтому каждый раз нужен новый экземпляр.
                        services.AddSingleton<DashboardViewModel>();
                        services.AddSingleton<DailyMenuListViewModel>();
                        services.AddSingleton<RecipeCardsViewModel>();
                        services.AddSingleton<StockViewModel>();
                        services.AddSingleton<DocumentsViewModel>();
                        services.AddTransient<DailyMenuEditorViewModel>();

                        // Справочники создаются при каждом открытии: так список всегда показывает
                        // актуальные данные, а форма не хранит состояние прошлой записи.
                        services.AddTransient<AgeGroupsReferenceViewModel>();
                        services.AddTransient<ChildGroupsReferenceViewModel>();
                        services.AddTransient<DishCategoriesReferenceViewModel>();
                        services.AddTransient<MealTypesReferenceViewModel>();
                        services.AddTransient<ProductsReferenceViewModel>();
                        services.AddTransient<SeasonsReferenceViewModel>();
                        services.AddTransient<StorageLocationsReferenceViewModel>();
                        services.AddTransient<DishesReferenceViewModel>();

                        //   services.AddScoped<ProductTabViewModel>();
                        //   services.AddScoped<BaseUnitTabViewModel>();

                        services.AddSingleton<App>();
                    }).
                    Build();

                // Всё разрешается из корневого провайдера; область (scope) скрывала бы реальное время
                // жизни и держала бы DbContext живым всю сессию.
                var app = host.Services.GetRequiredService<App>();
                app?.Run();
            }
            catch (Exception exception)
            {
                // Log.Fatal — последняя возможность записать, почему процесс не смог запуститься.
                Log.Fatal(exception, "Приложение завершилось с непредвиденной ошибкой");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}