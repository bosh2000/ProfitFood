using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Recipes;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class RecipeCardConfiguration : IEntityTypeConfiguration<RecipeCard>
    {
        public void Configure(EntityTypeBuilder<RecipeCard> builder)
        {
            builder.ToTable("RecipeCards");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CardNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.HasOne(x => x.Dish).WithMany(x => x.RecipeCards).HasForeignKey(x => x.DishId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.CardNumber).IsUnique();
        }
    }
}