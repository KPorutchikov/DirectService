using CSharpFunctionalExtensions;
using DirectService.Application.Database;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectService.Application.Positions.Command.Delete;

public class SoftDeletePositionHandler
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeletePositionHandler> _logger;

    public SoftDeletePositionHandler(IPositionRepository positionRepository
        , ITransactionManager transactionManager
        , ILogger<SoftDeletePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(Guid positionId, CancellationToken cancellationToken)
    {
        // Откроем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure) return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var positionLock = await _positionRepository.SetLockPositionSql(positionId, cancellationToken);
        if (positionLock.IsFailure)
        {
            transactionScope.Rollback();
            return positionLock.Error.ToErrors();
        }

        // Проверим существование позиции
        var position = await _positionRepository.GetById(positionId, cancellationToken);
        if (position.IsFailure)
        {
            transactionScope.Rollback();
            return position.Error.ToErrors();
        }

        position.Value.SetActive(false);
        var res = await _transactionManager.SaveChangesAsync(cancellationToken);

        // Коммитим транзакцию
        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation("Position {positionId} has been (soft)deleted", positionId);
        return positionId;

    }
}