using DirectService.Application.Locations;
using DirectService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectService.Presentation.Locations;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<Guid> CreateAsync(
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDto location,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handler(location, cancellationToken); 

        return result.Value;
    }
}