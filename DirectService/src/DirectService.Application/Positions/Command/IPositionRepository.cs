using CSharpFunctionalExtensions;
using DirectService.Domain.Departments;
using DirectService.Domain.Positions;
using Shared;

namespace DirectService.Application.Positions.Command;

public interface IPositionRepository
{
    public Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default);

    public Task<Result<Position?, Error>> GetByName(string name, CancellationToken cancellationToken = default);

    public Task<Result<Position?, Error>> GetById(Guid positionId, CancellationToken cancellationToken = default);
    
    public Task<Result<Guid, Error>> AddPositionToDepartment(Guid positionId, IEnumerable<Department> departments,
        CancellationToken cancellationToken = default);

    public Task<Result<Guid, Error>> SetLockPositionSql(Guid positionId, CancellationToken cancellationToken = default);
}