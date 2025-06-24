using Microsoft.EntityFrameworkCore;
using ProfitFood.Model.DBModel;

namespace ProfitFoot.DAL
{
    public class ProfitFoodDbContext : DbContext
    {
        public ProfitFoodDbContext(DbContextOptions options): base(options) 
        {
            
        }

        public DbSet<ProductGroup> ProductGroups { get; set; }

    }
}
