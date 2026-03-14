using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class MenuRequirementConfiguration : IEntityTypeConfiguration<MenuRequirement>
    {
        public void Configure(EntityTypeBuilder<MenuRequirement> builder)
        {
            builder.ToTable("MenuRequirements");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Number).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasOne(x => x.DailyMenu).WithOne(x => x.MenuRequirement).HasForeignKey<MenuRequirement>(x => x.DailyMenuId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StorageLocation).WithMany(x => x.MenuRequirements).HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HeadSigner).WithMany(x => x.HeadSignedRequirements).HasForeignKey(x => x.HeadSignerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StorekeeperSigner).WithMany(x => x.StorekeeperSignedRequirements).HasForeignKey(x => x.StorekeeperSignerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.MedWorkerSigner).WithMany(x => x.MedWorkerSignedRequirements).HasForeignKey(x => x.MedWorkerSignerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.Number, x.RequirementDate }).IsUnique();
            builder.HasIndex(x => x.DailyMenuId).IsUnique();
        }
    }
}