using DirectService.Application.Validation;
using DirectService.Domain.Departments;
using DirectService.Domain.Locations;
using FluentValidation;

namespace DirectService.Application.Departments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(x => x.Request.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.Request.LocationIds)
            .NotEmpty().WithMessage("Location is not be empty.").WithErrorCode("location.is.empty");
    }
}