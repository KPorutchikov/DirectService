using DirectService.Contracts.Locations;
using Shared.Abstractions;

namespace DirectService.Application.Locations.Command.Create;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;