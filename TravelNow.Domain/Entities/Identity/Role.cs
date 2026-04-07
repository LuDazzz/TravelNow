using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities.Identity;

[Table("Roles")]
public class Role : IdentityRole<Guid>, IAuditEntity
{
    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public virtual ICollection<RoleClaim> RoleClaims { get; set; } = [];
}