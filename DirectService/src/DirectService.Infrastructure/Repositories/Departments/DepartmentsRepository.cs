using System.Text;
using CSharpFunctionalExtensions;
using DirectService.Application.Departments;
using DirectService.Domain.Departments;
using DirectService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectService.Infrastructure.Repositories.Departments;

public class DepartmentsRepository: IDepartmentRepository
{
    private readonly DirectServiceDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(DirectServiceDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken = default)
    {
        _dbContext.Departments.Add(department);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return department.Id;

        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation && pgEx.ConstraintName != null)
            {
                if (pgEx.ConstraintName.Contains("department_name", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Error.Failure("department.add", "Fail to insert Location : duplicate value in column", "department_name");
                }

                if (pgEx.ConstraintName.Contains("identifier", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Error.Failure("department.add", "Fail to insert Location : duplicate value in column", "identifier");
                }
            }
            _logger.LogError(ex, "Database update error when creating location {name}", department.DepartmentName.Value);
            return Error.Failure("department.add", "Fail to insert Location : " + ex.Message);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating location {name}", department.DepartmentName.Value);
            return Error.Failure("department.add", "Operation was cancelled while creating location "+ department.DepartmentName.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to insert Location : " + e.Message);
            return Error.Failure("department.add", "Fail to insert Location : " + e.Message);
        }
    }

    public async Task<Result<Department, Error>> GetById(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .Where(d => d.Id == departmentId && d.IsActive == true)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (department == null)
            return Error.NotFound("value.not.found","Department is not found.");

        return department;
    }
    
    public async Task<Result<Department, Error>> GetByIdWithLocations(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .Include(d => d.Locations)
            .Where(d => d.Id == departmentId && d.IsActive == true)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (department == null)
            return Error.NotFound("value.not.found","Department is not found.");

        return department;
    }

    public async Task<Result<int, Error>> AddLocationsSql(Guid departmentId, IEnumerable<DepartmentLocation> departmentLocations, CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder("INSERT INTO department_locations (id, department_id, location_id, created_at) VALUES ");
      
        sql.Append(string.Join(",", departmentLocations.Select(l => $"('{l.Id}','{l.Department.Id}','{l.LocationId}','{l.CreatedAt}')")) + ";");
        
        try
        {
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql.ToString(),cancellationToken);
            
            return rowsAffected;
        }
        catch (Exception e)
        {
            return Error.Failure("database", e.Message);
        }
    }

    public async Task<Result<int, Error>> DeleteLocationsSql(Guid departmentId, IEnumerable<Guid> locationsIds, CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder($"DELETE FROM department_locations WHERE department_id = '{departmentId}' AND location_id IN ");
      
        sql.Append("(" + string.Join(",", locationsIds.Select(x => $"'{x}'")) + ");");
        
        try
        {
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql.ToString(),cancellationToken);
            
            return rowsAffected;
        }
        catch (Exception e)
        {
            return Error.Failure("database", e.Message);
        }
    }
}