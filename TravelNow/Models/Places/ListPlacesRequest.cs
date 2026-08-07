using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Places;

public sealed class ListPlacesRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    public Guid? ProvinceId { get; init; }

    [StringLength(200)]
    public string? Keyword { get; init; }
}
