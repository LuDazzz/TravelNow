using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;

namespace TravelNow.Domain.Entities;

[Table("Provinces")]
public class Province : BaseEntity
{
    public required string Name { get; set; }

    public string? BackgroundUrl { get; set; }

    public virtual ICollection<ProvinceMedia> ProvinceMedias { get; set; } = [];

    public virtual ICollection<Place> Places { get; set; } = [];
}