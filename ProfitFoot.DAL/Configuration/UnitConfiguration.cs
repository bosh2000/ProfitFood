using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.ToTable("Units");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ShortName).HasMaxLength(20).IsRequired();
            builder.Property(x => x.BaseFactor).HasPrecision(18, 6);
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.ShortName).IsUnique();
        }
    }
}