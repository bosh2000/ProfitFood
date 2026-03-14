namespace ProfitFood.Infrastructure.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ProfitFood.Domain.Entities.Menus;

    public sealed class CycleMenuConfiguration : IEntityTypeConfiguration<CycleMenu>
    {
        public void Configure(EntityTypeBuilder<CycleMenu> builder)
        {
            builder.ToTable("CycleMenus");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
            builder.HasOne(x => x.Season).WithMany(x => x.CycleMenus).HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AgeGroup).WithMany(x => x.CycleMenus).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.SeasonId, x.AgeGroupId, x.Name });
        }
    }
}