using CSharpFunctionalExtensions;
using DirectService.Domain.Locations;
using Shared;

namespace DirectService.Application.Locations.Command;

public interface ILocationsRepository
{
    public Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken = default);
    
    public Task<Result<Location, Error>> GetById(Guid locationId, CancellationToken cancellationToken = default);
    
    public Task<Result<Guid, Error>> SetLockLocationSql(Guid locationId, CancellationToken cancellationToken = default);
}