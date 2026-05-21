using DirectService.Application.Departments;
using DirectService.Application.Departments.Move;
using DirectService.Application.Departments.UpdateLocations;
using DirectService.Contracts.Departments;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

namespace DirectService.Presentation.Controllers.Departments;

[ApiController]
[Route("/api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> CreateAsync(
        [FromServices] CreateDepartmentHandler handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);
        
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("{id:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocationsAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromServices] UpdateDepartmentLocationsHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationsCommand(id, request);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPost("{id:guid}/parent")]
    public async Task<EndpointResult<Guid>> MoveDepartmentsAsync(
        [FromRoute] Guid id,
        [FromBody] MoveDepartmentsRequest request,
        [FromServices] MoveDepartmentHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new MoveDepartmentCommand(id, request.NewParentId);
        
        return await handler.Handle(command, cancellationToken);
    }
}