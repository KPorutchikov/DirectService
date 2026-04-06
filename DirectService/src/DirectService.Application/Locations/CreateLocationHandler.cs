using CSharpFunctionalExtensions;
using DirectService.Contracts.Locations;
using DirectService.Domain.Locations;
using FluentValidation;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared;
using TimeZone = DirectService.Domain.Locations.TimeZone;

namespace DirectService.Application.Locations;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationDto> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationsRepository locationsRepository
        , IValidator<CreateLocationDto> validator
        , ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handler(CreateLocationDto locationDto, CancellationToken ct)
    {
         var validationResult = await _validator.ValidateAsync(locationDto, ct);
         if (!validationResult.IsValid)
         {
             var error = Error.Validation(validationResult.Errors
                 .Select(e => new ErrorMessage(e.ErrorCode, e.ErrorMessage, e.PropertyName)));

             _logger.LogError("Validate a location is failed: {err}", JsonSerializer.Serialize(error));

             return error.ToErrors();
         }
        
        var locationId = Guid.NewGuid();
        
        var location = Location.Create(
            locationId, 
            LocationName.Create(locationDto.Name).Value, 
            Address.Create(locationDto.Address).Value, 
            TimeZone.Create(locationDto.TimeZone).Value);
        
        await _locationsRepository.Add(location.Value, ct);

        _logger.LogInformation("Location {id} has been created", locationId);

        return locationId;
    }
}