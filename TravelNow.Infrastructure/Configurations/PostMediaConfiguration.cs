using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("PostMedias");

        builder.HasKey(pm => pm.Id);

        // Property configurations
        builder.Property(pm => pm.Url).IsRequired().HasMaxLength(1000);

        // Relationships
        builder.HasOne(pm => pm.Post)
            .WithMany(p => p.PostMedias)
            .HasForeignKey(pm => pm.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pm => pm.PostId);
    }
}
