using Microsoft.EntityFrameworkCore;
using ProfitFood.Model.DBModel;

namespace ProfitFoot.DAL
{
    public class ProfitFoodDbContext : DbContext
    {
        public ProfitFoodDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BaseUnit> BaseUnits { get; set; }
        public DbSet<BaseUnitStorage> BaseUnitsStorage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(x => x.Group)
                .WithMany()
                .HasForeignKey(x => x.GroupId);
            modelBuilder.Entity<ProductGroup>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId);
            modelBuilder.Entity<BaseUnit>()
                .HasMany(x => x.Products)
                .WithOne(x => x.BaseUnit)
                .HasForeignKey(x => x.BaseUnitId);
            modelBuilder.Entity<BaseUnitStorage>()
                .HasMany(x => x.Products)
                .WithOne(x => x.BaseUnitStorage)
                .HasForeignKey(x => x.BaseUnitStorageId);
        }
    }
}