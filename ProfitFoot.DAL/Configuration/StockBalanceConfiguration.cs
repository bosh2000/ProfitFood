using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Documents;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
    {
        public void Configure(EntityTypeBuilder<StockBalance> builder)
        {
            builder.ToTable("StockBalances");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).HasPrecision(18, 3);
            builder.HasOne(x => x.StorageLocation).WithMany(x => x.StockBalances).HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Product).WithMany(x => x.StockBalances).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Unit).WithMany(x => x.StockBalances).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.StorageLocationId, x.ProductId }).IsUnique();
        }
    }
}