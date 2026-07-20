using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Locations.Command;
using DirectService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;
using Path = DirectService.Domain.Departments.Path;

namespace DirectService.Application.Departments.Commands.Create;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(IDepartmentRepository departmentRepository
        , ILocationsRepository locationsRepository
        , IValidator<CreateDepartmentCommand> validator
        , ILogger<CreateDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var error = Error.Validation(validationResult.Errors
                .Select(e => new ErrorMessage(e.ErrorCode ?? "value.is.invalid", e.ErrorMessage, e.PropertyName)));
         
            _logger.LogError("Validate a department is failed: {err}", JsonSerializer.Serialize(error));
            return error.ToErrors();
        }
        
        var departmentId = Guid.NewGuid();

        var identifier = Identifier.Create(request.Identifier).Value;
        var path = Path.Create(identifier.Value);
        short depth = 0;
        Guid? parentId = null;
        
        if (request.ParentId != null)
        {
            var parentDepartment = await _departmentRepository.GetById(request.ParentId.Value, cancellationToken);
            if (parentDepartment.IsFailure)
                return parentDepartment.Error.ToErrors();

            parentId = parentDepartment.Value.Id;
            path = parentDepartment.Value.Path.CreateChild(identifier.Value);
            depth = (short)(parentDepartment.Value.Depth!.Value + 1);
        }

        foreach (var locationId in request.LocationIds)
        {
            var resultLocation = await _locationsRepository.GetById(locationId, cancellationToken);
            if (resultLocation.IsFailure)
                return resultLocation.Error.ToErrors();
        }
        
        var department = Department.Create(
                departmentId,
                parentId,
                DepartmentName.Create(request.Name).Value,
                Identifier.Create(request.Identifier).Value,
                path,
                depth,
                request.LocationIds);

        var result = await _departmentRepository.Add(department.Value, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToErrors();

        _logger.LogInformation("Department {id} has been created", departmentId);
        
        return departmentId;
    }
}