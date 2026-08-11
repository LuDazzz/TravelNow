using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Domain.Entities;

[Table("UserInteractions")]
public class UserInteraction : IAuditEntity
{
    public Guid UserId { get; set; }

    public required virtual User User { get; set; }

    public Guid PostId { get; set; }

    public required virtual Post Post { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}