using FluentValidation;

namespace DirectService.Application.Departments.UpdateLocations;

public class CreateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public CreateDepartmentLocationsValidator()
    {
        RuleFor(x => x.Request.OldLocationIds)
            .NotEmpty().WithMessage("Old location is not be empty.").WithErrorCode("location.is.empty");
        
        RuleFor(x => x.Request.NewLocationIds)
            .NotEmpty()
            .WithMessage("New locations is not be empty.").WithErrorCode("location.is.empty")
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .WithMessage("New locations must not contains duplicates.").WithErrorCode("location.is.duplicate")
            .Must((command, newIds) => !newIds.Intersect(command.Request.OldLocationIds).Any())
            .WithMessage("New locations must not contains any old locations.").WithErrorCode("location.contains.old");
    }
}