using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities.Identity;

[Table("Roles")]
public class Role : IdentityRole<Guid>, IAuditEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}