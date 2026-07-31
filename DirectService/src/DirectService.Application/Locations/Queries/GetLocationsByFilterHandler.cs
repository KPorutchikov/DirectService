using System.Data;
using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using DirectService.Contracts.Locations;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Locations.Queries;

public class GetLocationsByFilterHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationsByFilterHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }
    
    public async Task<Result<PagedList<GetLocationFilterDto?>, Errors>> Handle(GetLocationFilterRequest filterRequest, CancellationToken ct)
    {
        var parameters = new DynamicParameters();

        long? totalCount = null;
        
        var connection  = await _dbConnection.CreateConnectionAsync(ct);

        var sql = new StringBuilder(
            """
            SELECT l.id, l.location_name, l.address, l.created_at, COUNT(dl.id) as department_count, COUNT(*) OVER() As total_count
            FROM locations l
            LEFT JOIN department_locations dl ON dl.location_id = l.id
            WHERE 1 = 1
            """);
        
        if (!string.IsNullOrWhiteSpace(filterRequest.Name))
        {
            sql.Append(" AND l.location_name ILIKE @Name");
            parameters.Add("Name", $"%{filterRequest.Name}%");
        }

        sql.Append(" GROUP BY l.id, l.location_name, l.address, l.created_at");
        
        if (filterRequest.minDepartmentCount.HasValue)
        {
            if (filterRequest.minDepartmentCount < 0)
            {
                _logger.LogInformation($"MinDepartmentCount cannot be negative");
                return Error.Failure("location", $"MinDepartmentCount cannot be negative").ToErrors();
            }
            sql.Append(" HAVING COUNT(dl.id) >= @MinDepartmentCount");
            parameters.Add("MinDepartmentCount", filterRequest.minDepartmentCount, DbType.Int32);
        }

        if (!string.IsNullOrWhiteSpace(filterRequest.SortByColumns))
        {
            var orderByField = filterRequest.SortByColumns?.ToLower() switch
            {
                "date" => "created_at",
                "name" => "location_name",
                "address" => "address",
                "department_count" => "department_count",
                _ => ""
            };

            if (orderByField == "")
            {
                _logger.LogInformation($"SortColumn is not found");
                return Error.Failure("location", $"SortColumn is not found").ToErrors();
            }
            
            sql.Append($" ORDER BY {orderByField} {(filterRequest.SortDir == 1 ? "DESC" : "ASC")}");
        }
        
        if (filterRequest.Page != null) 
        {
            if (filterRequest.Page < 0 || filterRequest.PageSize <= 0 || filterRequest.PageSize > 100)
            {
                _logger.LogInformation($"Page(size) is not valid");
                return Error.Failure("location", $"Page(size) is not valid").ToErrors();
            }
            sql.Append(" LIMIT @PageSize OFFSET @Offset");
            parameters.Add("@PageSize", filterRequest.PageSize, DbType.Int32);
            parameters.Add("@Offset", (filterRequest.Page - 1) * filterRequest.PageSize, DbType.Int32);
        }
        
        _logger.LogInformation($"SQL: {sql}");
        
        var locationDto = await connection.QueryAsync<GetLocationFilterDto, long, GetLocationFilterDto>(
                sql.ToString(),
                splitOn: "total_count",
                map: (@location, count) =>
                {
                    totalCount ??= count;
                    return @location;
                },
                param: parameters);
        
        if (!locationDto.Any())
        {
            _logger.LogInformation($"Locations not found");
            return Error.NotFound("location", $"Locations not found").ToErrors();
        }

        return new PagedList<GetLocationFilterDto>()
        {
            Items = locationDto.ToList(),
            TotalCount = totalCount ?? 0,
            PageSize = filterRequest.PageSize  ?? 0,
            Page = filterRequest.Page ?? 0,
        };
    }
}