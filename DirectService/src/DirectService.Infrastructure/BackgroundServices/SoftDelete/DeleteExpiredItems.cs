using Dapper;
using DirectService.Application.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectService.Infrastructure.BackgroundServices.SoftDelete;

public class DeleteExpiredItems
{
    private readonly ILogger<DeleteExpiredItems> _logger;
    private readonly IDbConnectionFactory _dbConnection;
    private readonly IOptions<SoftDeleteOptions> _softDeleteOptions;

    public DeleteExpiredItems(ILogger<DeleteExpiredItems> logger, IDbConnectionFactory dbConnection, IOptions<SoftDeleteOptions> softDeleteOptions)
    {
        _logger = logger;
        _dbConnection = dbConnection;
        _softDeleteOptions = softDeleteOptions;
    }

    public async Task ProcessAsync(CancellationToken stoppingToken)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(stoppingToken);

        var expirationDate = DateTime.UtcNow.AddDays(-1*_softDeleteOptions.Value.ExpiredDaysToRemove);
        
        try
        {
            var sqlDepartments = "DELETE FROM departments WHERE is_active = false AND updated_at < @expirationDate";
            var sqlLocations = "DELETE FROM locations WHERE is_active = false AND updated_at < @expirationDate";
            var sqlPositions = "DELETE FROM positions WHERE is_active = false AND updated_at < @expirationDate";

            var countDepartments = await connection.ExecuteAsync(sqlDepartments, param: new { expirationDate });
            var countLocations = await connection.ExecuteAsync(sqlLocations, param: new { expirationDate });
            var countPositions = await connection.ExecuteAsync(sqlPositions, param: new { expirationDate });
            
            _logger.LogInformation("Deleted expired items: departments={countDepartments}, locations={countLocations}, positions={countPositions}",
                countDepartments, countLocations, countPositions);
        }
        catch (Exception e)
        {
            _logger.LogInformation($"Deleting expired items failed: {e.Message}");
        }
    }
}