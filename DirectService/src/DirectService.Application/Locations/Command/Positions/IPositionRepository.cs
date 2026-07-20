using CSharpFunctionalExtensions;
using DirectService.Domain.Departments;
using DirectService.Domain.Positions;
using Shared;

namespace DirectService.Application.Locations.Command.Positions;

public interface IPositionRepository
{
    public Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default);

    public Task<Result<Position?, Error>> GetByName(string name, CancellationToken cancellationToken = default);

    public Task<Result<Guid, Error>> AddPositionToDepartment(Guid positionId, IEnumerable<Department> departments,
        CancellationToken cancellationToken = default);
}