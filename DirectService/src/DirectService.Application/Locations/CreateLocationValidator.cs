using DirectService.Contracts.Locations;
using FluentValidation;

namespace DirectService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500).WithMessage("Заголовок не валидный");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500).WithMessage("Адресс не валидный");
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(500).WithMessage("Timezone не валидный");
    }
}