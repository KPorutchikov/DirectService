using DirectService.Contracts.Locations;
using FluentValidation;

namespace DirectService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is not be empty.").WithErrorCode("name.is.empty")
            .Must(i => i.Length is >= 3 and <= 120 ).WithMessage("Name must be between 3 and 120 characters.").WithErrorCode("name.is.length");
        
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is not be empty.").WithErrorCode("address.is.empty")
            .MaximumLength(500).WithMessage("Address must be less 500 characters.").WithErrorCode("address.max.length");
        
        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is not be empty.").WithErrorCode("timezone.is.empty")
            .MaximumLength(100).WithMessage("TimeZone must be less 100 characters.").WithErrorCode("timezone.max.length");
    }
}