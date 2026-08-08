using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using DirectService.Application.Locations.Queries;
using DirectService.Contracts.Departments;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Departments.Queries.Trees;

public class GetDepartmentsRootHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;


    public GetDepartmentsRootHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<Result<GetDepartmentsTreeDto[]?, Errors>> Handle(CancellationToken ct)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(ct);
        
        var departmentsDto = await connection.QueryAsync<GetDepartmentsTreeDto>(
            """
            SELECT d.id, d.department_name, d.identifier, d.path, d.depth, (c.CountChildren > 0) as hasChildren
            FROM departments d
            CROSS JOIN LATERAL(SELECT COUNT(*) as CountChildren FROM departments dd WHERE dd.is_active = true AND dd.parent_id = d.id) c
            WHERE d.is_active = true AND d.depth = 0
            """);
        
        if (departmentsDto == null)
        {
            _logger.LogInformation($"Root departments are not found");
            return Error.NotFound("department", "Root departments are not found").ToErrors();
        }

        return departmentsDto.ToArray();
    }
}