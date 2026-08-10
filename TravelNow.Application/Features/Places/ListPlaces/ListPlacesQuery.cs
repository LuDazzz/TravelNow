namespace TravelNow.Application.Features.Places.ListPlaces;

public sealed record ListPlacesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? ProvinceId = null,
    string? Keyword = null);
