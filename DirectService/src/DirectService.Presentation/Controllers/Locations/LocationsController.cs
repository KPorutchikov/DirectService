using CSharpFunctionalExtensions;
using DirectService.Application.Locations;
using DirectService.Application.Locations.Command;
using DirectService.Application.Locations.Queries;
using DirectService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.DTO;
using Shared.EndpointResults;

namespace DirectService.Presentation.Controllers.Locations;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpGet("{locationId:guid}")]
    public async Task<EndpointResult<GetLocationDto?>> GetById(
        [FromRoute] Guid locationId,
        [FromServices] GetLocationByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(locationId, cancellationToken);
    }
    
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