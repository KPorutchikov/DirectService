using FluentValidation;

namespace DirectService.Application.Departments.Commands.Move;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId is not be empty.").WithErrorCode("department.is.empty")
            .Must((dep1, dep2) => dep1.NewParentId is null || dep1.NewParentId.Value != dep2)
            .WithMessage("DepartmentId is not be equal ParentId.").WithErrorCode("department.is.incorrect");
    }
}