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
        [FromBody] CreateLocationRequest location,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(location);
        
        return await handler.Handle(command, cancellationToken);
    }
}