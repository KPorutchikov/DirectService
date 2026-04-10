using DirectService.Contracts.Locations;
using Shared.Abstractions;

namespace DirectService.Application.Locations;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;