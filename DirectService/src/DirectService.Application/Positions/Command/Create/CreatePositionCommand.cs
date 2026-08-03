using DirectService.Contracts.Positions;
using Shared.Abstractions;

namespace DirectService.Application.Positions.Command.Create;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;