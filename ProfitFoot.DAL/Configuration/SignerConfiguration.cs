using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class SignerConfiguration : IEntityTypeConfiguration<Signer>
    {
        public void Configure(EntityTypeBuilder<Signer> builder)
        {
            builder.ToTable("Signers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Position).HasMaxLength(200).IsRequired();
            builder.HasIndex(x => new { x.FullName, x.Position });
        }
    }
}