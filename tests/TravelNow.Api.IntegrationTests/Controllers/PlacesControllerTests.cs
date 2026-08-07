using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TravelNow.Application.Abstractions.Persistence.Places;
using TravelNow.Application.Features.Places.ListPlaces;
using Xunit;

namespace TravelNow.Api.IntegrationTests.Controllers;

public sealed class PlacesControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetPlaces_returns_paginated_place_contract()
    {
        var placeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var provinceId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var testFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPlaceReadPort>();
                services.AddSingleton<IPlaceReadPort>(
                    new StubPlaceReadPort(new ListPlacesResult(
                        [new PlaceListItem(placeId, "Ha Long Bay", provinceId, "Quang Ninh", "Ha Long")],
                        TotalCount: 1,
                        Page: 1,
                        PageSize: 20)));
            }));

        using var client = testFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/places?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PlacesResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body!.TotalCount);
        Assert.Single(body.Items);
        Assert.Equal(placeId, body.Items[0].Id);
        Assert.Equal("Quang Ninh", body.Items[0].ProvinceName);
    }

    [Fact]
    public async Task GetPlaces_rejects_page_size_above_contract_limit()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/places?pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPlaces_hides_unexpected_exception_details()
    {
        var testFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPlaceReadPort>();
                services.AddSingleton<IPlaceReadPort>(new ThrowingPlaceReadPort());
            }));

        using var client = testFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/places");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("provider secret", body, StringComparison.Ordinal);
        Assert.Contains("traceId", body, StringComparison.Ordinal);
    }

    private sealed class StubPlaceReadPort(ListPlacesResult result) : IPlaceReadPort
    {
        public Task<ListPlacesResult> ListAsync(ListPlacesQuery query, CancellationToken cancellationToken)
            => Task.FromResult(result);
    }

    private sealed class ThrowingPlaceReadPort : IPlaceReadPort
    {
        public Task<ListPlacesResult> ListAsync(ListPlacesQuery query, CancellationToken cancellationToken)
            => throw new InvalidOperationException("provider secret: database connection details");
    }

    private sealed record PlacesResponse(
        IReadOnlyList<PlaceResponse> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);

    private sealed record PlaceResponse(
        Guid Id,
        string Name,
        Guid ProvinceId,
        string ProvinceName,
        string? Location);
}
