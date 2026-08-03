using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Departments;
using DirectService.Application.Departments.Commands;
using DirectService.Domain.Departments;
using DirectService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Shared;
using Shared.DTO;

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
                    return Error.Failure("department.add", "Fail to insert : duplicate value in column", "department_name");
                }

                if (pgEx.ConstraintName.Contains("identifier", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Error.Failure("department.add", "Fail to insert : duplicate value in column", "identifier");
                }
            }
            _logger.LogError(ex, "Database update error when creating department {name}", department.DepartmentName.Value);
            return Error.Failure("department.add", "Fail to insert department : " + ex.Message);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating department {name}", department.DepartmentName.Value);
            return Error.Failure("department.add", "Operation was cancelled while creating department "+ department.DepartmentName.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to insert department : " + e.Message);
            return Error.Failure("department.add", "Fail to insert department : " + e.Message);
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
      
        sql.Append(string.Join(",", departmentLocations.Select(l => $"('{l.Id}','{l.Department.Id}','{l.LocationId}', CURRENT_TIMESTAMP)")) + ";");
        
        try
        {
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql.ToString(),cancellationToken);
            
            return rowsAffected;
        }
        catch (Exception e)
        {
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
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
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
        }
    }
   
    public async Task<Result<Guid, Error>> SetLockDepartmentLocationSql(Guid departmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            /* _dbContext.Database.ExecuteSqlAsync($"SELECT department_id FROM department_locations WHERE department_id = {departmentId} FOR UPDATE", cancellationToken); */

            var departmentLocationResult = await _dbContext.DepartmentLocations
                .FromSql($"SELECT * FROM department_locations WHERE department_id = {departmentId} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);
            
            if (departmentLocationResult == null) return GeneralErrors.NotFound(departmentId, "department_location");

            return departmentId;
        }
        catch (Exception e)
        {
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
        }
    }

    public async Task<Result<List<DepartmentsLockDto>, Error>> SetLockDepartmentTree(Guid departmentId, Guid? newParentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var departments = await _dbContext.Database.SqlQuery<DepartmentsLockDto>($@"
                SELECT id AS Id, parent_id AS ParentId, identifier AS Identifier, path AS Path 
                FROM departments 
                WHERE is_active = true AND 
                      (path <@ (SELECT path FROM departments WHERE id = {departmentId} AND is_active = true) OR 
                      (id = {newParentId} AND is_active = true))
                FOR UPDATE")
                .ToListAsync(cancellationToken);
            
            if (departments.Count == 0)
                return GeneralErrors.NotFound(departmentId);

            return departments;
        }
        catch (Exception e)
        {
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
        }
    }
    
    public async Task<Result<int, Error>> UpdateDepartmentPathTree(string rootPath, Guid? newParentId, CancellationToken cancellationToken)
    {
        try
        {
            var rootPathParam    = new NpgsqlParameter("rootPath", NpgsqlDbType.LTree) { Value = rootPath };
            var newParentIdParam = new NpgsqlParameter("newParentId", NpgsqlDbType.Uuid) { Value = (object?)newParentId ?? DBNull.Value };
            
            const string sql = @"
            WITH cte_departments AS (
            SELECT
	            d.id,
	            CASE WHEN d.path = @rootPath::ltree THEN n.id ELSE d.parent_id END AS NewParentId,
	            COALESCE(n.path,'') || subpath(d.path, index(d.path, text2ltree(r.identifier)), nlevel(d.path)) as NewPath
            FROM departments d
            LEFT JOIN departments r ON r.is_active = true AND r.path = @rootPath::ltree
            LEFT JOIN departments o ON o.is_active = true AND o.id = r.parent_id
            LEFT JOIN departments n ON n.is_active = true AND n.id = @newParentId
            WHERE d.is_active = true AND d.path <@ @rootPath::ltree)

            UPDATE departments d
            SET parent_id  = n.NewParentId,
	            path       = n.NewPath,
	            depth      = nlevel(n.NewPath)-1,
	            updated_at = CURRENT_TIMESTAMP
            FROM cte_departments n 
            WHERE n.id = d.id";
            
            int rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql, new object[] {rootPathParam, newParentIdParam}, cancellationToken);

            if (rowsAffected == 0)
                return GeneralErrors.Failure(rootPath);
            
            return rowsAffected;
        }
        catch (Exception e)
        {
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
        }
    }
}