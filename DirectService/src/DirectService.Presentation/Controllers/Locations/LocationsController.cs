using DirectService.Application.Locations;
using DirectService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

namespace DirectService.Presentation.Controllers.Locations;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> CreateAsync(
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDto location,
        CancellationToken cancellationToken)
    {
        return await handler.Handler(location, cancellationToken);
    }
}