using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Recipes;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            builder.ToTable("RecipeIngredients");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GrossQuantity).HasPrecision(18, 3);
            builder.Property(x => x.NetQuantity).HasPrecision(18, 3);
            builder.Property(x => x.LossPercent).HasPrecision(5, 2);
            builder.HasOne(x => x.RecipeCardVersion).WithMany(x => x.Ingredients).HasForeignKey(x => x.RecipeCardVersionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Product).WithMany(x => x.RecipeIngredients).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Unit).WithMany(x => x.RecipeIngredients).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.RecipeCardVersionId, x.SortOrder });
        }
    }
}