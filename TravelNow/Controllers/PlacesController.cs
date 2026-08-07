using Microsoft.AspNetCore.Mvc;
using TravelNow.Application.Features.Places.ListPlaces;
using TravelNow.Models.Places;

namespace TravelNow.Controllers;

[ApiController]
[Route("api/places")]
public sealed class PlacesController(ListPlacesHandler listPlacesHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListPlacesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListPlacesResponse>> GetPlaces(
        [FromQuery] ListPlacesRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ListPlacesQuery(
            request.Page,
            request.PageSize,
            request.ProvinceId,
            request.Keyword);

        var result = await listPlacesHandler.HandleAsync(query, cancellationToken);
        return Ok(ListPlacesResponse.FromResult(result));
    }
}
