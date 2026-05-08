using CSharpFunctionalExtensions;
using DirectService.Application.Database;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectService.Infrastructure.Database;

public class TransactionManager: ITransactionManager
{
    private readonly DirectServiceDbContext _dbContext;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public TransactionManager(DirectServiceDbContext dbContext, ILogger<TransactionManager> logger, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }
    
    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transactionScopeLogger = _loggerFactory.CreateLogger<TransactionScope>();
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);
            
            return transactionScope;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes transaction");
            return Error.Failure("database", "Failed to begin transaction");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return UnitResult.Success<Error>();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes");
            return Error.Failure("database", $"Failed to save changes. {ex.Message}");
        }
        
    }
    
}