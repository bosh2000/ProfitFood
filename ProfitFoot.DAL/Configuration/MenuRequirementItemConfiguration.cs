using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class MenuRequirementItemConfiguration : IEntityTypeConfiguration<MenuRequirementItem>
    {
        public void Configure(EntityTypeBuilder<MenuRequirementItem> builder)
        {
            builder.ToTable("MenuRequirementItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).HasPrecision(18, 3);
            builder.Property(x => x.Price).HasPrecision(18, 2);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.HasOne(x => x.MenuRequirement).WithMany(x => x.Items).HasForeignKey(x => x.MenuRequirementId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Product).WithMany(x => x.MenuRequirementItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Unit).WithMany(x => x.MenuRequirementItems).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.MenuRequirementId, x.SortOrder });
        }
    }
}