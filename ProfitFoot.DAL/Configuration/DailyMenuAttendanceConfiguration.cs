using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.Menus;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class DailyMenuAttendanceConfiguration : IEntityTypeConfiguration<DailyMenuAttendance>
    {
        public void Configure(EntityTypeBuilder<DailyMenuAttendance> builder)
        {
            builder.ToTable("DailyMenuAttendances");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.DailyMenu).WithMany(x => x.Attendances).HasForeignKey(x => x.DailyMenuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.ChildGroup).WithMany(x => x.DailyMenuAttendances).HasForeignKey(x => x.ChildGroupId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.DailyMenuId, x.ChildGroupId }).IsUnique();
        }
    }
}