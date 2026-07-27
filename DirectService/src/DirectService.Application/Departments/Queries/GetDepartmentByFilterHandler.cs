using System.Data;
using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using DirectService.Application.Locations.Queries;
using DirectService.Contracts.Departments;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Departments.Queries;

public class GetDepartmentByFilterHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetDepartmentByFilterHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<Result<PagedList<GetDepartmentFilterDto?>, Errors>> Handle(GetDepartmentRequest request, CancellationToken ct)
    {
        var parameters = new DynamicParameters();

        long? totalCount = null;
        
        var connection  = await _dbConnection.CreateConnectionAsync(ct);

        var sortColumns = new List<string> { "department_name"};
            
        var sql = new StringBuilder(
            """
            SELECT  d.id, d.department_name, d.path, d.created_at, COUNT(*) OVER() as total_count
            FROM departments d
            WHERE 1 = 1
            """);
        
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            sql.Append(" AND d.department_name ILIKE @Name");
            parameters.Add("Name", $"%{request.Name}%");
        }
        
        if (!string.IsNullOrWhiteSpace(request.SortByColumns))
        {
            if (!sortColumns.Contains(request.SortByColumns))
            {
                _logger.LogInformation($"SortColumn is not found");
                return Error.NotFound("department", $"SortColumn is not found").ToErrors();
            }
            
            sql.Append(" ORDER BY @SortBy");
            parameters.Add("@SortBy", $"{request.SortByColumns} {(request.SortDir == 0 ? "ASC" : "DESC")}" ?? "1");
        }

        if (request.Page != null) 
        {
            if (request.Page < 0 || request.PageSize <= 0 || request.PageSize > 100)
            {
                _logger.LogInformation($"Page(size) is not valid");
                return Error.Failure("department", $"Page(size) is not valid").ToErrors();
            }
            sql.Append(" LIMIT @PageSize OFFSET @Offset");
            parameters.Add("@PageSize", request.PageSize, DbType.Int32);
            parameters.Add("@Offset", (request.Page - 1) * request.PageSize, DbType.Int32);
        }
        
        _logger.LogInformation($"SQL: {sql}");
        
        var departmentDto = await connection.QueryAsync<GetDepartmentFilterDto, long, GetDepartmentFilterDto>(
                sql.ToString(),
                splitOn: "total_count",
                map: (@department, count) =>
                {
                    totalCount ??= count;
                    return @department;
                },
                param: parameters);
        
        if (!departmentDto.Any())
        {
            _logger.LogInformation($"Departments not found");
            return Error.NotFound("department", $"Departments not found").ToErrors();
        }

        return new PagedList<GetDepartmentFilterDto>()
        {
            Items = departmentDto.ToList(),
            TotalCount = totalCount ?? 0,
            PageSize = request.PageSize  ?? 0,
            Page = request.Page ?? 0,
        };
    }
}