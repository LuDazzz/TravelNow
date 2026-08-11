using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities;

[Table("Places")]
public class Place : BaseEntity
{
    public required string Name { get; set; }

    public Guid ProvinceId { get; set; }

    public required virtual Province Province { get; set; }

    public string? Location { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = [];

    public virtual ICollection<PlaceTag> PlaceTags { get; set; } = [];
}