using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Documents;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class StockDocumentConfiguration : IEntityTypeConfiguration<StockDocument>
    {
        public void Configure(EntityTypeBuilder<StockDocument> builder)
        {
            builder.ToTable("StockDocuments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasOne(x => x.StorageLocation).WithMany(x => x.StockDocuments).HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RelatedMenuRequirement).WithMany(x => x.StockDocuments).HasForeignKey(x => x.RelatedMenuRequirementId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.DocumentNumber, x.DocumentDate, x.StorageLocationId }).IsUnique();
        }
    }
}