namespace TravelNow.Application.Features.Places.ListPlaces;

public sealed record ListPlacesResult(
    IReadOnlyList<PlaceListItem> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
