using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using DirectService.Application.Locations.Queries;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Departments.Queries.Trees;

public class GetDepartmentsByNameHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetDepartmentsByNameHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }
    
    public async Task<Result<GetDepartmentsTreeDto[]?, Errors>> Handle(string departmentName, CancellationToken ct)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(ct);
        
        var departmentsDto = await connection.QueryAsync<GetDepartmentsTreeDto>(
            """
            SELECT d.id, d.department_name, d.identifier, d.path, d.depth, (c.CountСhildren > 0) as hasChildren
            FROM departments d
            CROSS JOIN LATERAL(SELECT COUNT(*) as CountСhildren FROM departments dd WHERE dd.is_active = true AND dd.parent_id = d.id) c
            WHERE d.is_active = true AND d.department_name ILIKE @DepartmentName
            ORDER BY d.depth, d.path
            """, param: new { DepartmentName = $"%{departmentName}%"});

        return departmentsDto.ToArray();
    }
}