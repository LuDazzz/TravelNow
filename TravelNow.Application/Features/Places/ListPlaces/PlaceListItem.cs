namespace TravelNow.Application.Features.Places.ListPlaces;

public sealed record PlaceListItem(
    Guid Id,
    string Name,
    Guid ProvinceId,
    string ProvinceName,
    string? Location);
