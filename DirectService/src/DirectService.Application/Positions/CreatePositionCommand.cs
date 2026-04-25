using DirectService.Contracts.Positions;
using Shared.Abstractions;

namespace DirectService.Application.Positions;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;