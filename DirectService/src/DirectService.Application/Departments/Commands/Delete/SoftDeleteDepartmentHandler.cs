using CSharpFunctionalExtensions;
using DirectService.Application.Database;
using DirectService.Application.Departments.Commands.Move;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectService.Application.Departments.Commands.Delete;

public class SoftDeleteDepartmentHandler
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;

    public SoftDeleteDepartmentHandler(
        IDepartmentRepository departmentRepository
        , ITransactionManager transactionManager
        , ILogger<SoftDeleteDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(Guid idDepartment, CancellationToken cancellationToken)
    {
         // Откроем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure) return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        var departmentLock = await _departmentRepository.SetLockDepartmentLocationSql(idDepartment, cancellationToken);
        if (departmentLock.IsFailure)
        {
            transactionScope.Rollback();
            return departmentLock.Error.ToErrors(); 
        }
        
        // Проверим существование департамента
        var department = await _departmentRepository.GetById(idDepartment, cancellationToken);
        if (department.IsFailure)
        {
            transactionScope.Rollback();
            return department.Error.ToErrors(); 
        }

        department.Value.SetActive(false);
        var res = await _transactionManager.SaveChangesAsync(cancellationToken);

        // Коммитим транзакцию
        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitedResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Department {idDepartment} has been (soft)deleted", idDepartment);
        return idDepartment;
    }
}