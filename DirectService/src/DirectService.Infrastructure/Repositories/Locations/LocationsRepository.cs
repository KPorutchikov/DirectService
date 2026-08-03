using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Locations;
using DirectService.Application.Locations.Command;
using DirectService.Domain.Locations;
using DirectService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectService.Infrastructure.Repositories.Locations;

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
        _dbContext.Locations.Add(location);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return location.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation && pgEx.ConstraintName != null)
            {
                if (pgEx.ConstraintName.Contains("location_name", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Error.Failure("location.add", "Fail to insert Location : duplicate value in column", "location_name");
                }

                if (pgEx.ConstraintName.Contains("address", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Error.Failure("location.add", "Fail to insert Location : duplicate value in column", "address");
                }
            }
            _logger.LogError(ex, "Database update error when creating location {name}", location.Name.Value);
            return Error.Failure("location.add", "Fail to insert Location : " + ex.Message);
        }
        
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating location {name}", location.Name.Value);
            return Error.Failure("location.add", "Operation was cancelled while creating location "+ location.Name.Value);
        }
        
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to insert Location : " + e.Message);
            return Error.Failure("location.add", "Fail to insert Location : " + e.Message);
        }
    }

    public async Task<Result<Location, Error>> GetById(Guid locationId, CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations
            .Where(l => l.Id == locationId && l.IsActive == true)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (location == null)
            return Error.NotFound("value.not.found","Location is not found.");

        return location;
    }
    
    public async Task<Result<Guid, Error>> SetLockLocationSql(Guid locationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentLocationResult = await _dbContext.Locations
                .FromSql($"SELECT * FROM locations WHERE is_active = true AND id = {locationId} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);
            
            if (departmentLocationResult == null) return GeneralErrors.NotFound(locationId, "location");

            return locationId;
        }
        catch (Exception e)
        {
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
        }
    }
}