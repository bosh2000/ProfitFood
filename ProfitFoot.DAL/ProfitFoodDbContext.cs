using Microsoft.EntityFrameworkCore;
using ProfitFood.Domain.Entities.Menus;
using ProfitFood.Domain.Entities.References;
using ProfitFood.Domain.Entities.Recipes;
using ProfitFood.Domain.Entities.Documents;
using ProfitFood.Domain.Entities;

namespace ProfitFoot.Infrastructure
{
    public class ProfitFoodDbContext : DbContext
    {
        public ProfitFoodDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
        public DbSet<AgeGroup> AgeGroups => Set<AgeGroup>();
        public DbSet<ChildGroup> ChildGroups => Set<ChildGroup>();
        public DbSet<MealType> MealTypes => Set<MealType>();
        public DbSet<DishCategory> DishCategories => Set<DishCategory>();
        public DbSet<Dish> Dishes => Set<Dish>();
        public DbSet<RecipeCard> RecipeCards => Set<RecipeCard>();
        public DbSet<RecipeCardVersion> RecipeCardVersions => Set<RecipeCardVersion>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<DishOutput> DishOutputs => Set<DishOutput>();
        public DbSet<Season> Seasons => Set<Season>();
        public DbSet<CycleMenu> CycleMenus => Set<CycleMenu>();
        public DbSet<CycleMenuDay> CycleMenuDays => Set<CycleMenuDay>();
        public DbSet<CycleMenuItem> CycleMenuItems => Set<CycleMenuItem>();
        public DbSet<DailyMenu> DailyMenus => Set<DailyMenu>();
        public DbSet<DailyMenuAttendance> DailyMenuAttendances => Set<DailyMenuAttendance>();
        public DbSet<DailyMenuItem> DailyMenuItems => Set<DailyMenuItem>();
        public DbSet<Signer> Signers => Set<Signer>();
        public DbSet<MenuRequirement> MenuRequirements => Set<MenuRequirement>();
        public DbSet<MenuRequirementItem> MenuRequirementItems => Set<MenuRequirementItem>();
        public DbSet<StockDocument> StockDocuments => Set<StockDocument>();
        public DbSet<StockDocumentItem> StockDocumentItems => Set<StockDocumentItem>();
        public DbSet<StockBalance> StockBalances => Set<StockBalance>();
        public DbSet<ProfitFood.Domain.Entities.AppSettings> AppSettings => Set<AppSettings>();
        public DbSet<DatabaseSettings> DatabaseSettings => Set<DatabaseSettings>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProfitFoodDbContext).Assembly);
        }
    }
}