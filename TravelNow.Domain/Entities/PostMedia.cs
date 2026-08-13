using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities;

[Table("PostMedias")]
public class PostMedia : BaseEntity
{
    public required string Url { get; set; }

    public Guid PostId { get; set; }

    public required virtual Post Post { get; set; }
}