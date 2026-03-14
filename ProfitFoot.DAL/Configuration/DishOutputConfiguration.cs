using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class DishOutputConfiguration : IEntityTypeConfiguration<DishOutput>
    {
        public void Configure(EntityTypeBuilder<DishOutput> builder)
        {
            builder.ToTable("DishOutputs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OutputQuantity).HasPrecision(18, 3);
            builder.HasOne(x => x.RecipeCardVersion).WithMany(x => x.DishOutputs).HasForeignKey(x => x.RecipeCardVersionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.AgeGroup).WithMany(x => x.DishOutputs).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Unit).WithMany(x => x.DishOutputs).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.RecipeCardVersionId, x.AgeGroupId }).IsUnique();
        }
    }
}