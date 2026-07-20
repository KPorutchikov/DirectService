using Shared.Abstractions;

namespace DirectService.Application.Departments.Commands.Move;

public record MoveDepartmentCommand(Guid DepartmentId, Guid? NewParentId) : ICommand;