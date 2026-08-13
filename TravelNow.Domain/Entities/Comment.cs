using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Domain.Entities;

[Table("Comments")]
public class Comment : BaseEntity
{
    public Guid UserId { get; set; }

    public required virtual User User { get; set; }

    public Guid PostId { get; set; }

    public required virtual Post Post { get; set; }

    public Guid? ParentId { get; set; }

    public virtual Comment? ParentComment { get; set; }

    public virtual ICollection<Comment> Replies { get; set; } = [];

    public required string Content { get; set; }
}