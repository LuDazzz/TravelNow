using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Domain.Entities;

[Table("Posts")]
public class Post : BaseEntity
{
    public string? Title { get; set; }

    public double? Rating { get; set; }

    public Guid PlaceId { get; set; }

    public virtual Place Place { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    public string? Content { get; set; }

    public virtual ICollection<PostMedia> PostMedias { get; set; } = [];

    public virtual ICollection<PostTag> PostTags { get; set; } = [];

    public virtual ICollection<Comment> Comments { get; set; } = [];

    public virtual ICollection<UserInteraction> UserInteractions { get; set; } = [];
}
