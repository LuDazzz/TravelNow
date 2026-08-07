using TravelNow.Application.Features.Places.ListPlaces;

namespace TravelNow.Application.Abstractions.Persistence.Places;

public interface IPlaceReadPort
{
    Task<ListPlacesResult> ListAsync(ListPlacesQuery query, CancellationToken cancellationToken);
}
