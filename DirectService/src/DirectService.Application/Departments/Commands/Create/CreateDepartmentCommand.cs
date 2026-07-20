using DirectService.Contracts.Departments;
using Shared.Abstractions;

namespace DirectService.Application.Departments.Commands.Create;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;