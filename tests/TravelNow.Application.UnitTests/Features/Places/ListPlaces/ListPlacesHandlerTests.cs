using TravelNow.Application.Abstractions.Persistence.Places;
using TravelNow.Application.Features.Places.ListPlaces;
using Xunit;

namespace TravelNow.Application.UnitTests.Features.Places.ListPlaces;

public sealed class ListPlacesHandlerTests
{
    [Fact]
    public async Task HandleAsync_normalizes_paging_and_keyword_before_reading()
    {
        var port = new RecordingPlaceReadPort(
            new ListPlacesResult(
                [new PlaceListItem(Guid.NewGuid(), "Ha Long Bay", Guid.NewGuid(), "Quang Ninh", "Ha Long")],
                TotalCount: 101,
                Page: 1,
                PageSize: 100));
        var handler = new ListPlacesHandler(port);

        var result = await handler.HandleAsync(
            new ListPlacesQuery(Page: 1, PageSize: 250, ProvinceId: null, Keyword: "  bay  "),
            CancellationToken.None);

        Assert.NotNull(port.Query);
        Assert.Equal(100, port.Query!.PageSize);
        Assert.Equal("bay", port.Query.Keyword);
        Assert.Equal(101, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task HandleAsync_forwards_cancellation_token_to_read_port()
    {
        var port = new RecordingPlaceReadPort(
            new ListPlacesResult([], TotalCount: 0, Page: 1, PageSize: 20));
        var handler = new ListPlacesHandler(port);
        using var cancellation = new CancellationTokenSource();

        await handler.HandleAsync(new ListPlacesQuery(), cancellation.Token);

        Assert.Equal(cancellation.Token, port.CancellationToken);
    }

    private sealed class RecordingPlaceReadPort(ListPlacesResult result) : IPlaceReadPort
    {
        public ListPlacesQuery? Query { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<ListPlacesResult> ListAsync(ListPlacesQuery query, CancellationToken cancellationToken)
        {
            Query = query;
            CancellationToken = cancellationToken;
            return Task.FromResult(result);
        }
    }
}
