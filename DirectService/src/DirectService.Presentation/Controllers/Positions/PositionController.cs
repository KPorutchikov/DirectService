using DirectService.Application.Positions;
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
}