using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Enums;

namespace TravelNow.Domain.Entities;

[Table("Tags")]
public class Tag : BaseEntity
{
    public required string Title { get; set; }

    public TagEnum Type { get; set; }

    public virtual ICollection<PostTag> PostTags { get; set; } = [];

    public virtual ICollection<PlaceTag> PlaceTags { get; set; } = [];
}
