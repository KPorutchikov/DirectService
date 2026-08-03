using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Positions.Command;
using DirectService.Domain.Departments;
using DirectService.Domain.Positions;
using DirectService.Infrastructure.Database;
using DirectService.Infrastructure.Repositories.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectService.Infrastructure.Repositories.Positions;

public class PositionsRepository : IPositionRepository
{
    private readonly DirectServiceDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(DirectServiceDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken = default)
    {
        _dbContext.Positions.Add(position);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return position.Id;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating position {name}", position.Name);
            return Error.Failure("position.add", "Operation was cancelled while creating position "+position.Name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to insert Position : " + e.Message);
            return Error.Failure("position.add", "Fail to insert Position : " + e.Message);
        }
    }

    public async Task<Result<Position?, Error>> GetByName(string name, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Positions.FirstOrDefaultAsync(p => p.Name == name && p.IsActive == false, cancellationToken);
            
        return result;
    }
    
    public async Task<Result<Position?, Error>> GetById(Guid positionId, CancellationToken cancellationToken = default)
    {
        var position = await _dbContext.Positions
            .Where(p => p.Id == positionId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (position == null)
            return Error.NotFound("value.not.found","Position is not found.");

        return position;
    }

    public async Task<Result<Guid, Error>> AddPositionToDepartment(Guid positionId, IEnumerable<Department> departments, CancellationToken cancellationToken = default)
    {
        foreach (var department in departments)
        {
            await _dbContext.DepartmentPositions.AddAsync(new DepartmentPosition(Guid.NewGuid(), department, positionId), cancellationToken);
        }
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return positionId;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating links department with position {id}", positionId);
            return Error.Failure("departmentposition.add", "was cancelled while creating links department with position "+positionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to insert DepartmentPosition : " + e.Message);
            return Error.Failure("departmentposition.add", "Fail to insert DepartmentPosition : " + e.Message);
        }
    }
    
    public async Task<Result<Guid, Error>> SetLockPositionSql(Guid positionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dbContext.Positions
                .FromSql($"SELECT * FROM positions WHERE is_active = true AND id = {positionId} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);
            
            if (result == null) return GeneralErrors.NotFound(positionId, "position");

            return positionId;
        }
        catch (Exception e)
        {
            return Error.Failure("database", JsonSerializer.Serialize(e.Message));
        }
    }
}