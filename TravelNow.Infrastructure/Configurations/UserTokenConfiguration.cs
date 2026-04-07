using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Infrastructure.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens");

        // Property configurations
        builder.Property(ut => ut.RefreshToken).HasMaxLength(500);
        builder.Property(ut => ut.DeviceInfo).HasMaxLength(500);
        builder.Property(ut => ut.LocationInfo).HasMaxLength(500);
    }
}
