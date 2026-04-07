using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class ProvinceMediaConfiguration : IEntityTypeConfiguration<ProvinceMedia>
{
    public void Configure(EntityTypeBuilder<ProvinceMedia> builder)
    {
        builder.ToTable("ProvinceMedias");

        builder.HasKey(pm => pm.Id);

        // Property configurations
        builder.Property(pm => pm.Url).IsRequired().HasMaxLength(1000);

        // Relationships
        builder.HasOne(pm => pm.Province)
            .WithMany(p => p.ProvinceMedias)
            .HasForeignKey(pm => pm.ProvinceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pm => pm.ProvinceId);
    }
}
