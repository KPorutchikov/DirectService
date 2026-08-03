using CSharpFunctionalExtensions;
using DirectService.Application.Database;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectService.Application.Locations.Command.Delete;

public class SoftDeleteLocationHandler
{
    private readonly ILocationsRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteLocationHandler> _logger;

    public SoftDeleteLocationHandler(ILocationsRepository locationRepository
        , ITransactionManager transactionManager
        , ILogger<SoftDeleteLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(Guid locationId, CancellationToken cancellationToken)
    {
        // Откроем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure) return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var departmentLock = await _locationRepository.SetLockLocationSql(locationId, cancellationToken);
        if (departmentLock.IsFailure)
        {
            transactionScope.Rollback();
            return departmentLock.Error.ToErrors();
        }

        // Проверим существование локации
        var location = await _locationRepository.GetById(locationId, cancellationToken);
        if (location.IsFailure)
        {
            transactionScope.Rollback();
            return location.Error.ToErrors();
        }

        location.Value.SetActive(false);
        var res = await _transactionManager.SaveChangesAsync(cancellationToken);

        // Коммитим транзакцию
        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitedResult.Error.ToErrors();
        }

        _logger.LogInformation("Location {locationId} has been (soft)deleted", locationId);
        return locationId;
    }
}