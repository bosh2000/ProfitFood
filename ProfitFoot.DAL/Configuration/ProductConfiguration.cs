using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Article).HasMaxLength(100);
            builder.Property(x => x.MinStock).HasPrecision(18, 3);
            builder.HasOne(x => x.ProductCategory).WithMany(x => x.Products).HasForeignKey(x => x.ProductCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.BaseUnit).WithMany(x => x.BaseProducts).HasForeignKey(x => x.BaseUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StorageUnit).WithMany(x => x.StorageProducts).HasForeignKey(x => x.StorageUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => new { x.ProductCategoryId, x.Name });
        }
    }
}