using CSharpFunctionalExtensions;
using DirectService.Application.Locations;
using DirectService.Domain.Locations;
using DirectService.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectService.Infrastructure.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectServiceDbContext _dbContext;
    private readonly ILogger<LocationsRepository> _logger;


    public LocationsRepository(DirectServiceDbContext dbContext, ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        
            return location.Id;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to insert Location : " + e.Message);
            
            return Error.Failure("location.add", "Fail to insert Location : " + e.Message);
        }
    }
}