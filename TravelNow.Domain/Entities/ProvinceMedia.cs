using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities;

[Table("ProvinceMedias")]
public class ProvinceMedia : BaseEntity
{
    public required string Url { get; set; }

    public Guid ProvinceId { get; set; }

    public required virtual Province Province { get; set; }
}