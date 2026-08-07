using TravelNow.Application.Abstractions.Persistence.Places;

namespace TravelNow.Application.Features.Places.ListPlaces;

public sealed class ListPlacesHandler(IPlaceReadPort placeReadPort)
{
    private const int MaximumPageSize = 100;

    public async Task<ListPlacesResult> HandleAsync(
        ListPlacesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Page), "Page must be at least 1.");
        }

        if (query.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.PageSize), "Page size must be at least 1.");
        }

        var normalizedQuery = query with
        {
            PageSize = Math.Min(query.PageSize, MaximumPageSize),
            Keyword = string.IsNullOrWhiteSpace(query.Keyword) ? null : query.Keyword.Trim()
        };

        return await placeReadPort.ListAsync(normalizedQuery, cancellationToken);
    }
}
