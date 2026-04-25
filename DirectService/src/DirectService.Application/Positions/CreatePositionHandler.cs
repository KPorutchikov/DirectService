using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectService.Application.Departments;
using DirectService.Domain.Departments;
using DirectService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;

namespace DirectService.Application.Positions;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionRepository positionRepository,
        IDepartmentRepository departmentRepository,
        IValidator<CreatePositionCommand> validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _departmentRepository = departmentRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var positionRequest = command.Request;
        var departments = new List<Department>();

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var error = Error.Validation(validationResult.Errors
                .Select(e => new ErrorMessage(e.ErrorCode ?? "value.is.invalid", e.ErrorMessage, e.PropertyName)));
         
            _logger.LogError("Validate a position is failed: {err}", JsonSerializer.Serialize(error));
            return error.ToErrors();
        }
        
        var positionId = Guid.NewGuid();
        
        var checkExistPositionName = _positionRepository.GetByName(positionRequest.Name, cancellationToken).Result.Value;
        if (checkExistPositionName != null)
            return Error.Conflict("record.already.exist", "Position name already exists").ToErrors();
        
        if(positionRequest.DepartmentIds.Length != positionRequest.DepartmentIds.ToHashSet().Count)
            return Error.NotFound("department.is.exists",$"Departments are repeated in the request").ToErrors();
        
        foreach (var departmentId in positionRequest.DepartmentIds)
        {
            var resultLocation = await _departmentRepository.GetById(departmentId, cancellationToken);
            if (resultLocation.IsFailure)
                return Error.NotFound("record.not.exists",$"Department with id {departmentId} not found").ToErrors();
            
            departments.Add(resultLocation.Value);
        }
        
        var position = Position.Create(positionId,positionRequest.Name, positionRequest.Description).Value;
        
        // начало транзакции
        var resultPosition = await _positionRepository.Add(position, cancellationToken);
        if (resultPosition.IsFailure)
            return resultPosition.Error.ToErrors();
        
        var resultPositionToDepartment = await _positionRepository.AddPositionToDepartment(
            positionId, departments, cancellationToken);
        if (resultPositionToDepartment.IsFailure)
            return resultPositionToDepartment.Error.ToErrors();
        // конец транзакции

        _logger.LogInformation("Position {id} has been created", positionId);

        return positionId;
    }
}