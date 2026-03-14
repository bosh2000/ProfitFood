namespace ProfitFood.Infrastructure.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ProfitFood.Domain.Entities;

    public sealed class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
    {
        public void Configure(EntityTypeBuilder<AppSettings> builder)
        {
            builder.ToTable("AppSettings");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InstitutionName).HasMaxLength(500).IsRequired();
            builder.Property(x => x.InstitutionShortName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Address).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Form299Title).HasMaxLength(250);
            builder.Property(x => x.DirectorName).HasMaxLength(250);
            builder.HasOne(x => x.DefaultStorageLocation).WithMany(x => x.SettingsAsDefaultStorage).HasForeignKey(x => x.DefaultStorageLocationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}