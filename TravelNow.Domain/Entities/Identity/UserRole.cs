using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities.Identity;

[Table("UserRoles")]
public class UserRole : IdentityUserRole<Guid>, IAuditEntity
{
    // IAuditEntity
    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
