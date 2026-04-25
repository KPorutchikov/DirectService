using DirectService.Application.Departments;
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
}