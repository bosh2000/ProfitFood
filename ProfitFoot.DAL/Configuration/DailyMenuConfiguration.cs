using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class DailyMenuConfiguration : IEntityTypeConfiguration<DailyMenu>
    {
        public void Configure(EntityTypeBuilder<DailyMenu> builder)
        {
            builder.ToTable("DailyMenus");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasOne(x => x.Season).WithMany(x => x.DailyMenus).HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CycleMenu).WithMany(x => x.DailyMenus).HasForeignKey(x => x.CycleMenuId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.MenuDate).IsUnique();
        }
    }
}