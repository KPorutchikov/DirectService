using DirectService.Application.Validation;
using DirectService.Contracts.Locations;
using DirectService.Domain.Locations;
using FluentValidation;

namespace DirectService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(LocationName.Create);
           
        RuleFor(x => x.Request.Address)
            .NotEmpty().WithMessage("Address is not be empty.").WithErrorCode("address.is.empty")
            .MaximumLength(500).WithMessage("Address must be less 500 characters.").WithErrorCode("address.max.length");
        
        RuleFor(x => x.Request.TimeZone)
            .NotEmpty().WithMessage("TimeZone is not be empty.").WithErrorCode("timezone.is.empty")
            .MaximumLength(100).WithMessage("TimeZone must be less 100 characters.").WithErrorCode("timezone.max.length");
    }
}