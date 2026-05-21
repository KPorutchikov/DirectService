using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Database;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;

namespace DirectService.Application.Departments.Move;

public class MoveDepartmentHandler: ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ILogger<MoveDepartmentHandler> _logger;

    public MoveDepartmentHandler(IDepartmentRepository departmentRepository
        , ITransactionManager transactionManager
        , IValidator<MoveDepartmentCommand> validator
        , ILogger<MoveDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
    {
        // Валидация вх. параметров
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var error = Error.Validation(validationResult.Errors
                .Select(e => new ErrorMessage(e.ErrorCode ?? "value.is.invalid", e.ErrorMessage, e.PropertyName)));
         
            _logger.LogError("Validate params are failed: {err}", JsonSerializer.Serialize(error));
            return error.ToErrors();
        }
        
        // Откроем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure) return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;
        
        // Получим коллекцию дочерних департаментов, которые будут затронуты переносом (и заблокируем их на время операции)
        var departmentsResult = await _departmentRepository.SetLockDepartmentTree(command.DepartmentId, command.NewParentId, cancellationToken);
        if (departmentsResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentsResult.Error.ToErrors(); 
        }

        // Проверка что целевой департамент существует 
        if(!departmentsResult.Value.Exists(x => x.Id == command.DepartmentId))
        {
            transactionScope.Rollback();
            _logger.LogInformation($"Department {command.DepartmentId} not found");
            return Error.Validation("department", $"Department {command.DepartmentId} not found").ToErrors();
        }
        
        // Проверка что новый родитель существует (если он указан в параметрах)
        if (command.NewParentId != null && !departmentsResult.Value.Exists(x => x.Id == command.NewParentId))
        {
            transactionScope.Rollback();
            _logger.LogInformation($"New parent of department {command.NewParentId} is not found");
            return Error.Validation("department", $"New parent of department {command.NewParentId} is not found").ToErrors();
        }
        
        // Проверка что указанный новый родитель не является потомком
        var oldPath = departmentsResult.Value.Single(x => x.Id == command.DepartmentId).Path;
        if(command.NewParentId != null && departmentsResult.Value.Exists(x => x.Id == command.NewParentId && x.Path.IndexOf(oldPath) >= 0))
        {
            transactionScope.Rollback();
            _logger.LogInformation($"Department {command.NewParentId} is child of {command.DepartmentId}");
            return Error.Validation("department", $"Department {command.NewParentId} is child of {command.DepartmentId}").ToErrors();
        }
        
        // Проверка что департамент не является уже корнем дерева (при попытке его таким сделать)
        if (command.NewParentId is null && departmentsResult.Value.Exists(x => x.Id == command.DepartmentId && x.Identifier == x.Path))
        {
            transactionScope.Rollback();
            _logger.LogInformation($"Department {command.DepartmentId} already is root of tree");
            return Error.Validation("department", $"Department {command.DepartmentId} already is root of tree").ToErrors();
        }
        
        // Обновим объекты в БД
        var updateResult = await _departmentRepository.UpdateDepartmentPathTree(oldPath, command.NewParentId, cancellationToken);
        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateResult.Error.ToErrors();
        }

        // Коммитим транзакцию
        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitedResult.Error.ToErrors();
        }
        
        return command.DepartmentId;
    }
}