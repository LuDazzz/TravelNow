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

        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Gender).HasMaxLength(20);
        builder.Property(u => u.Address).HasMaxLength(500);
        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.UserName).HasMaxLength(256);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IsDeleted);
    }
}
