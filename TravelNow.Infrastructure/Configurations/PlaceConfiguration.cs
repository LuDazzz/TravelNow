using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class PlaceConfiguration : IEntityTypeConfiguration<Place>
{
    public void Configure(EntityTypeBuilder<Place> builder)
    {
        builder.ToTable("Places");

        builder.HasKey(p => p.Id);

        // Property configurations
        builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Location).HasMaxLength(500);

        // Relationships
        builder.HasOne(p => p.Province)
            .WithMany(pr => pr.Places)
            .HasForeignKey(p => p.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Posts)
            .WithOne(po => po.Place)
            .HasForeignKey(po => po.PlaceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.ProvinceId);
    }
}
