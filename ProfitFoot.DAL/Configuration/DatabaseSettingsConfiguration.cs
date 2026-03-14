using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class DatabaseSettingsConfiguration : IEntityTypeConfiguration<DatabaseSettings>
    {
        public void Configure(EntityTypeBuilder<DatabaseSettings> builder)
        {
            builder.ToTable("DatabaseSettings");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProviderName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DatabaseFilePath).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
        }
    }
}