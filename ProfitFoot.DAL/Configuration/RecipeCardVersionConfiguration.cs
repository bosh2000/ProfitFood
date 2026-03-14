using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Recipes;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class RecipeCardVersionConfiguration : IEntityTypeConfiguration<RecipeCardVersion>
    {
        public void Configure(EntityTypeBuilder<RecipeCardVersion> builder)
        {
            builder.ToTable("RecipeCardVersions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OutputQuantity).HasPrecision(18, 3);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.HasOne(x => x.RecipeCard).WithMany(x => x.Versions).HasForeignKey(x => x.RecipeCardId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OutputUnit).WithMany(x => x.OutputRecipeVersions).HasForeignKey(x => x.OutputUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.RecipeCardId, x.VersionNumber }).IsUnique();
        }
    }
}