namespace TravelNow.Models.Places;

public sealed record PlaceResponse(
    Guid Id,
    string Name,
    Guid ProvinceId,
    string ProvinceName,
    string? Location);
