using DirectService.Application.Validation;
using DirectService.Domain.Positions;
using FluentValidation;

namespace DirectService.Application.Positions.Command.Create;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Request.Name)
            .MustBeValueObject(Position.CreateName);
        
        RuleFor(x => x.Request.Description)
            .MustBeValueObject(Position.CreateDescription);
        
        RuleFor(x => x.Request.DepartmentIds)
            .NotEmpty().WithMessage("Department is not be empty.").WithErrorCode("department.is.empty");
    }
    
}