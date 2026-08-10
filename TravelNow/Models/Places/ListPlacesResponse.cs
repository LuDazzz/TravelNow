using TravelNow.Application.Features.Places.ListPlaces;

namespace TravelNow.Models.Places;

public sealed record ListPlacesResponse(
    IReadOnlyList<PlaceResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static ListPlacesResponse FromResult(ListPlacesResult result)
    {
        var items = result.Items
            .Select(item => new PlaceResponse(
                item.Id,
                item.Name,
                item.ProvinceId,
                item.ProvinceName,
                item.Location))
            .ToArray();

        return new ListPlacesResponse(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);
    }
}
