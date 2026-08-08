using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using DirectService.Application.Locations.Queries;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Departments.Queries.Trees;

public class GetDepartmentChildrenHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;


    public GetDepartmentChildrenHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }
    
    public async Task<Result<GetDepartmentsTreeDto[]?, Errors>> Handle(Guid departmentId, CancellationToken ct)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(ct);
        
        var departmentsDto = await connection.QueryAsync<GetDepartmentsTreeDto>(
            """
            SELECT d.id, d.department_name, d.identifier, d.path, d.depth, 0 as hasChildren
            FROM departments d
            WHERE d.is_active = true AND d.parent_id = @departmentID
            """, param: new { departmentID = departmentId });

        if (departmentsDto == null)
        {
            _logger.LogInformation($"Department is not found");
            return Error.NotFound("department", "Department is not found").ToErrors();
        }
        
        departmentsDto = await connection.QueryAsync<GetDepartmentsTreeDto>(
            """
            SELECT d.id, d.department_name, d.identifier, d.path, d.depth, (c.CountСhildren > 0) as hasChildren
            FROM departments d
            CROSS JOIN LATERAL(SELECT COUNT(*) as CountСhildren FROM departments dd WHERE dd.is_active = true AND dd.parent_id = d.id) c
            WHERE d.is_active = true AND d.parent_id = @departmentID
            ORDER BY d.depth, d.path
            """, param: new { departmentID = departmentId });
        
        return departmentsDto.ToArray();
    }
    
    
}