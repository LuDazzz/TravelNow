using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelNow.Domain.Entities;

namespace TravelNow.Infrastructure.Configurations;

public class UserInteractionConfiguration : IEntityTypeConfiguration<UserInteraction>
{
    public void Configure(EntityTypeBuilder<UserInteraction> builder)
    {
        builder.ToTable("UserInteractions");

        // Composite primary key
        builder.HasKey(ui => new { ui.UserId, ui.PostId });

        // Relationships
        builder.HasOne(ui => ui.User)
            .WithMany(u => u.UserInteractions)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Post)
            .WithMany(p => p.UserInteractions)
            .HasForeignKey(ui => ui.PostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
