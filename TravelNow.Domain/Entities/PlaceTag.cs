using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities;

[Table("PlaceTags")]
public class PlaceTag : IAuditEntity
{
    public Guid PlaceId { get; set; }

    public required virtual Place Place { get; set; }

    public Guid TagId { get; set; }

    public required virtual Tag Tag { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}