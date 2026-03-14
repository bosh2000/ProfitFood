using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class DailyMenuItemConfiguration : IEntityTypeConfiguration<DailyMenuItem>
    {
        public void Configure(EntityTypeBuilder<DailyMenuItem> builder)
        {
            builder.ToTable("DailyMenuItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasOne(x => x.DailyMenu).WithMany(x => x.Items).HasForeignKey(x => x.DailyMenuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.MealType).WithMany(x => x.DailyMenuItems).HasForeignKey(x => x.MealTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Dish).WithMany(x => x.DailyMenuItems).HasForeignKey(x => x.DishId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecipeCardVersion).WithMany(x => x.DailyMenuItems).HasForeignKey(x => x.RecipeCardVersionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.DailyMenuId, x.MealTypeId, x.SortOrder });
        }
    }
}