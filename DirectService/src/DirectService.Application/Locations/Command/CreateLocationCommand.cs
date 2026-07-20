using DirectService.Contracts.Locations;
using Shared.Abstractions;

namespace DirectService.Application.Locations.Command;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;