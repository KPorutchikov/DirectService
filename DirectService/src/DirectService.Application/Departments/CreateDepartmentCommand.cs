using DirectService.Contracts.Departments;
using Shared.Abstractions;

namespace DirectService.Application.Departments;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;