using DirectService.Contracts.Departments;
using Shared.Abstractions;

namespace DirectService.Application.Departments.Commands.UpdateLocations;

public record UpdateDepartmentLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request): ICommand;