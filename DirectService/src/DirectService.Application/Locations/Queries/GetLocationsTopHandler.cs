using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Locations.Queries;

public class GetLocationsTopHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationsTopHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<Result<GetLocationTopDto[]?, Errors>> Handle(CancellationToken cancellationToken)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(cancellationToken);

        var locationsDto = await connection.QueryAsync<GetLocationTopDto>(
            """
              SELECT l.id, l.location_name, l.address, COUNT(dl.id) as DepartmentCount
              FROM locations l
              LEFT JOIN department_locations dl ON dl.location_id = l.id
              GROUP BY l.id, l.location_name
              ORDER BY 4 DESC, 1
              LIMIT 5
            """);
        
        if (!locationsDto.Any())
        {
            _logger.LogInformation($"Locations not found");
            return Error.NotFound("location", $"Locations not found").ToErrors();
        }

        return locationsDto.ToArray();
    }
}