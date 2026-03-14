using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class CycleMenuItemConfiguration : IEntityTypeConfiguration<CycleMenuItem>
    {
        public void Configure(EntityTypeBuilder<CycleMenuItem> builder)
        {
            builder.ToTable("CycleMenuItems");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.CycleMenuDay).WithMany(x => x.Items).HasForeignKey(x => x.CycleMenuDayId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.MealType).WithMany(x => x.CycleMenuItems).HasForeignKey(x => x.MealTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Dish).WithMany(x => x.CycleMenuItems).HasForeignKey(x => x.DishId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecipeCardVersion).WithMany(x => x.CycleMenuItems).HasForeignKey(x => x.RecipeCardVersionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.CycleMenuDayId, x.MealTypeId, x.SortOrder });
        }
    }
}