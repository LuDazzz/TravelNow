using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.HasKey(p => p.Id);

        // Property configurations
        builder.Property(p => p.Title).HasMaxLength(500);
        builder.Property(p => p.Content).HasColumnType("nvarchar(max)");
        builder.Property(p => p.Rating).HasColumnType("float");

        // Relationships
        builder.HasOne(p => p.Place)
            .WithMany(pl => pl.Posts)
            .HasForeignKey(p => p.PlaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PostMedias)
            .WithOne(pm => pm.Post)
            .HasForeignKey(pm => pm.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.PlaceId);
    }
}