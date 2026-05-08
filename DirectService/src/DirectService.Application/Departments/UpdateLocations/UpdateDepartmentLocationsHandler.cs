using System.Net.Security;
using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Database;
using DirectService.Application.Locations;
using DirectService.Domain.Departments;
using DirectService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;

namespace DirectService.Application.Departments.UpdateLocations;

public class UpdateDepartmentLocationsHandler: ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public UpdateDepartmentLocationsHandler(IDepartmentRepository departmentRepository
        , ILocationsRepository locationsRepository
        , ITransactionManager transactionManager
        , IValidator<UpdateDepartmentLocationsCommand> validator
        , ILogger<CreateDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentLocationsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var error = Error.Validation(validationResult.Errors
                .Select(e => new ErrorMessage(e.ErrorCode ?? "value.is.invalid", e.ErrorMessage, e.PropertyName)));
         
            _logger.LogError("Validate a department locations is failed: {err}", JsonSerializer.Serialize(error));
            return error.ToErrors();
        }

        // Откроем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure) return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        // Проверим существование департамента
        var department = await _departmentRepository.GetByIdWithLocations(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
        {
            transactionScope.Rollback();
            return department.Error.ToErrors(); 
        }

        // Добавим новые локации (предварительно проверив их наличие)
        var departmentLocations = new List<DepartmentLocation>();
        foreach (var newLocationId in command.Request.NewLocationIds)
        {
            var newLocation = await _locationsRepository.GetById(newLocationId, cancellationToken);
            if (newLocation.IsFailure)
            {
                transactionScope.Rollback();
                return newLocation.Error.ToErrors();
            }
            departmentLocations!.Add(DepartmentLocation.Create(department.Value, newLocationId).Value);
        }
        var addLocationResult = await _departmentRepository.AddLocationsSql(command.DepartmentId, departmentLocations!, cancellationToken);
        if (addLocationResult.IsFailure || addLocationResult.Value == 0)
        {
            transactionScope.Rollback();
            if (addLocationResult.Value == 0)
                return Error.Failure("database", "New locations could not be added").ToErrors();
            return addLocationResult.Error.ToErrors();
        }  

        // Удалим старые локации
        var deleteLocationResult = await _departmentRepository.DeleteLocationsSql(command.DepartmentId, command.Request.OldLocationIds, cancellationToken);
        if (deleteLocationResult.IsFailure || deleteLocationResult.Value == 0)
        {
            transactionScope.Rollback();
            if (deleteLocationResult.Value == 0)
                return Error.Failure("database","Locations could not be deleted").ToErrors();
            return deleteLocationResult.Error.ToErrors();
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