using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNow.Domain.Entities.Base;

public abstract class BaseEntity : IAuditEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = default!;

    public DateTimeOffset CreateAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid CreateBy { get; set; }

    public DateTimeOffset? UpdateAt { get; set; }

    public Guid? UpdateBy { get; set; }
}
