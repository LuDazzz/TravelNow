using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities;

[Table("PostTags")]
public class PostTag : IAuditEntity
{
    public Guid PostId { get; set; }

    public required virtual Post Post { get; set; }

    public Guid TagId { get; set; }

    public required virtual Tag Tag { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}