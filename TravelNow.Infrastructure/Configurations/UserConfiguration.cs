using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Property configurations
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Address).HasMaxLength(500);
        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.UserName).HasMaxLength(256);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);

        // Relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.HasMany(u => u.UserTokens)
            .WithOne(ut => ut.User)
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        builder.HasMany(u => u.Posts)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
