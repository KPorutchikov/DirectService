using CSharpFunctionalExtensions;
using DirectService.Contracts.Locations;
using DirectService.Domain.Locations;
using FluentValidation;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;
using TimeZone = DirectService.Domain.Locations.TimeZone;

namespace DirectService.Application.Locations;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationsRepository locationsRepository
        , IValidator<CreateLocationCommand> validator
        , ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
         var validationResult = await _validator.ValidateAsync(command, cancellationToken);
         if (!validationResult.IsValid)
         {
             var error = Error.Validation(validationResult.Errors
                 .Select(e => new ErrorMessage(e.ErrorCode ?? "value.is.invalid", e.ErrorMessage, e.PropertyName)));
         
             _logger.LogError("Validate a location is failed: {err}", JsonSerializer.Serialize(error));
         
             return error.ToErrors();
         }
        
        var locationId = Guid.NewGuid();
        
        var location = Location.Create(
            locationId, 
            LocationName.Create(request.Name).Value, 
            Address.Create(request.Address).Value, 
            TimeZone.Create(request.TimeZone).Value);
        if (location.IsFailure)
            return location.Error.ToErrors();

        
        var result = await _locationsRepository.Add(location.Value, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToErrors();

        _logger.LogInformation("Location {id} has been created", locationId);

        return locationId;
    }
}