using Microsoft.EntityFrameworkCore;
using TravelNow.Application.Abstractions.Persistence.Places;
using TravelNow.Application.Features.Places.ListPlaces;

namespace TravelNow.Infrastructure.Features.Places;

public sealed class PlaceReadPort(TravelNowDbContext dbContext) : IPlaceReadPort
{
    public async Task<ListPlacesResult> ListAsync(
        ListPlacesQuery query,
        CancellationToken cancellationToken)
    {
        var places = dbContext.Places.AsNoTracking();

        if (query.ProvinceId.HasValue)
        {
            places = places.Where(place => place.ProvinceId == query.ProvinceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var pattern = $"%{query.Keyword.Trim()}%";
            places = places.Where(place => EF.Functions.ILike(place.Name, pattern));
        }

        var totalCount = await places.CountAsync(cancellationToken);
        var skip = (int)Math.Min(
            (long)(query.Page - 1) * query.PageSize,
            int.MaxValue);

        var items = await places
            .OrderBy(place => place.Name)
            .ThenBy(place => place.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(place => new PlaceListItem(
                place.Id,
                place.Name,
                place.ProvinceId,
                place.Province.Name,
                place.Location))
            .ToListAsync(cancellationToken);

        return new ListPlacesResult(items, totalCount, query.Page, query.PageSize);
    }
}
