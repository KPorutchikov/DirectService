using Shared.Abstractions;

namespace DirectService.Application.Departments.Move;

public record MoveDepartmentCommand(Guid DepartmentId, Guid? NewParentId) : ICommand;