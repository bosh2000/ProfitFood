using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfitFood.Domain.Entities.References;

namespace ProfitFood.Infrastructure.Configurations
{
    public sealed class ChildGroupConfiguration : IEntityTypeConfiguration<ChildGroup>
    {
        public void Configure(EntityTypeBuilder<ChildGroup> builder)
        {
            builder.ToTable("ChildGroups");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.HasOne(x => x.AgeGroup).WithMany(x => x.ChildGroups).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}