using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Documents;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class StockDocumentItemConfiguration : IEntityTypeConfiguration<StockDocumentItem>
    {
        public void Configure(EntityTypeBuilder<StockDocumentItem> builder)
        {
            builder.ToTable("StockDocumentItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).HasPrecision(18, 3);
            builder.Property(x => x.Price).HasPrecision(18, 2);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.HasOne(x => x.StockDocument).WithMany(x => x.Items).HasForeignKey(x => x.StockDocumentId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Product).WithMany(x => x.StockDocumentItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Unit).WithMany(x => x.StockDocumentItems).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}