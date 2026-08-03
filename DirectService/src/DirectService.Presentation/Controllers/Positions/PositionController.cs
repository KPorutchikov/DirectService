using DirectService.Application.Positions.Command.Create;
using DirectService.Application.Positions.Command.Delete;
using DirectService.Contracts.Positions;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

namespace DirectService.Presentation.Controllers.Positions;

[ApiController]
[Route("/api/positions")]

public class PositionController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> CreateAsync(
        [FromServices] CreatePositionHandler handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpDelete("{positionId:guid}")]
    public async Task<EndpointResult<Guid>> SoftDelete(
        [FromRoute] Guid positionId,
        [FromServices] SoftDeletePositionHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(positionId, cancellationToken);
    }
}