using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class CycleMenuDayConfiguration : IEntityTypeConfiguration<CycleMenuDay>
    {
        public void Configure(EntityTypeBuilder<CycleMenuDay> builder)
        {
            builder.ToTable("CycleMenuDays");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.HasOne(x => x.CycleMenu).WithMany(x => x.Days).HasForeignKey(x => x.CycleMenuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.CycleMenuId, x.DayNumber }).IsUnique();
        }
    }
}