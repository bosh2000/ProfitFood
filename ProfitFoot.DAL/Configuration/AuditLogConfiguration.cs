namespace ProfitFood.Infrastructure.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ProfitFood.Domain.Entities;

    public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.OldValueJson).IsRequired();
            builder.Property(x => x.NewValueJson).IsRequired();
            builder.HasIndex(x => new { x.EntityName, x.EntityId, x.ChangedAt });
        }
    }
}