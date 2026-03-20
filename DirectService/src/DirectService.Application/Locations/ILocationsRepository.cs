using CSharpFunctionalExtensions;
using DirectService.Domain.Locations;
using Shared;

namespace DirectService.Application.Locations;

public interface ILocationsRepository
{
    public Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken = default);
    
}