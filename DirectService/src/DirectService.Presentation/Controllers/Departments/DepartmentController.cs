using CSharpFunctionalExtensions;
using DirectService.Application.Departments;
using DirectService.Application.Departments.Commands.Create;
using DirectService.Application.Departments.Commands.Delete;
using DirectService.Application.Departments.Commands.Move;
using DirectService.Application.Departments.Commands.UpdateLocations;
using DirectService.Application.Departments.Queries;
using DirectService.Application.Departments.Queries.Trees;
using DirectService.Contracts.Departments;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.DTO;
using Shared.EndpointResults;

namespace DirectService.Presentation.Controllers.Departments;

[ApiController]
[Route("/api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpGet("{departmentId:guid}")]
    public async Task<EndpointResult<GetDepartmentDto?>> GetById(
        [FromRoute] Guid departmentId,
        [FromServices] GetDepartmentByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(departmentId, cancellationToken);
    }
    
    [HttpGet("")]
    public async Task<EndpointResult<PagedList<GetDepartmentFilterDto?>>> GetByFilter(
        [FromServices] GetDepartmentByFilterHandler handler,
        [FromQuery] GetDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    
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

    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> SoftDelete(
        [FromRoute] Guid departmentId,
        [FromServices] SoftDeleteDepartmentHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(departmentId, cancellationToken);
    }
    
    [HttpGet("/departments/tree")]
    public async Task<EndpointResult<GetDepartmentsTreeDto[]?>> GetDepartmentsRoot(
        [FromServices] GetDepartmentsRootHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
    
    [HttpGet("/departments/{departmentId:guid}/children")]
    public async Task<EndpointResult<GetDepartmentsTreeDto[]?>> GetDepartmentChildren(
        [FromRoute] Guid departmentId,
        [FromServices] GetDepartmentChildrenHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(departmentId, cancellationToken);
    }
    
    [HttpGet("/departments/{departmentId:guid}/ancestors")]
    public async Task<EndpointResult<GetDepartmentsTreeDto[]?>> GetDepartmentHierarchy(
        [FromRoute] Guid departmentId,
        [FromServices] GetDepartmentHierarchyHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(departmentId, cancellationToken);
    }
    
    [HttpGet("/departments/tree/search")]
    public async Task<EndpointResult<GetDepartmentsTreeDto[]?>> GetDepartmentsByName(
        [FromQuery] string departmentName,
        [FromServices] GetDepartmentsByNameHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(departmentName, cancellationToken);
    }
}