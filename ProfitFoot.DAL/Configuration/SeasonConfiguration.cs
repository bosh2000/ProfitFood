using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.ToTable("Seasons");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasCheckConstraint("CK_Seasons_MonthFrom", "MonthFrom >= 1 AND MonthFrom <= 12");
            builder.HasCheckConstraint("CK_Seasons_MonthTo", "MonthTo >= 1 AND MonthTo <= 12");
        }
    }
}