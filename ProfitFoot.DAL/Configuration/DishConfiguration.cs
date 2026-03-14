using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class DishConfiguration : IEntityTypeConfiguration<Dish>
    {
        public void Configure(EntityTypeBuilder<Dish> builder)
        {
            builder.ToTable("Dishes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasOne(x => x.DishCategory).WithMany(x => x.Dishes).HasForeignKey(x => x.DishCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.MealType).WithMany(x => x.Dishes).HasForeignKey(x => x.MealTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.Name);
        }
    }
}