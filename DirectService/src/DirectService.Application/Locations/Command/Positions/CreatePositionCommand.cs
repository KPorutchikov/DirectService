using DirectService.Contracts.Positions;
using Shared.Abstractions;

namespace DirectService.Application.Locations.Command.Positions;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;