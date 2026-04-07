using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");

        builder.HasKey(p => p.Id);

        // Property configurations
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BackgroundUrl).HasMaxLength(1000);

        // Relationships
        builder.HasMany(p => p.Places)
            .WithOne(pl => pl.Province)
            .HasForeignKey(pl => pl.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ProvinceMedias)
            .WithOne(pm => pm.Province)
            .HasForeignKey(pm => pm.ProvinceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
