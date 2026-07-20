using CSharpFunctionalExtensions;
using Dapper;
using DirectService.Application.Database;
using DirectService.Application.Locations.Queries;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Departments.Queries;

public class GetDepartmentByIdHandler
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetDepartmentByIdHandler(IDbConnectionFactory dbConnection, ILogger<GetLocationByIdHandler> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<Result<GetDepartmentDto?, Errors>> Handle(Guid id, CancellationToken cancellationToken)
    {
        var connection  = await _dbConnection.CreateConnectionAsync(cancellationToken);
        
        var departmentDto = await connection.QueryFirstOrDefaultAsync<GetDepartmentDto>(
            """
              SELECT id, department_name, identifier, is_active, created_at, updated_at
              FROM departments
              WHERE id = @departmentId
            """, param: new { departmentId = id });

        if (departmentDto == null)
        {
            _logger.LogInformation($"Department {id} not found");
            return Error.NotFound("department", $"Department {id} not found").ToErrors();
        }

        return  departmentDto;
    }
}