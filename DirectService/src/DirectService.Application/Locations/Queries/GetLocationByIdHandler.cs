using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Locations.Queries;

public class GetLocationByIdHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationByIdHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<Result<GetLocationDto?, Errors>> Handle(Guid id, CancellationToken cancellationToken)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(cancellationToken);
        
        var locationDto = await connection.QueryFirstOrDefaultAsync<GetLocationDto>(
            """
              SELECT id, location_name, address, timezone, is_active, created_at, updated_at
              FROM locations
              WHERE id = @locationId
            """, param: new { locationId = id });

        if (locationDto == null)
        {
            _logger.LogInformation($"Location {id} not found");
            return Error.NotFound("location", $"Location {id} not found").ToErrors();
        }

        return  locationDto;
    }
}