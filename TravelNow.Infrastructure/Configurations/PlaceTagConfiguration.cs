using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class PlaceTagConfiguration : IEntityTypeConfiguration<PlaceTag>
{
    public void Configure(EntityTypeBuilder<PlaceTag> builder)
    {
        builder.ToTable("PlaceTags");

        // Composite primary key
        builder.HasKey(pt => new { pt.PlaceId, pt.TagId });

        // Relationships
        builder.HasOne(pt => pt.Place)
            .WithMany(p => p.PlaceTags)
            .HasForeignKey(pt => pt.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.PlaceTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(pt => !pt.Place.IsDeleted && !pt.Tag.IsDeleted);
    }
}
