using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateMembreDtoValidator : AbstractValidator<CreateMembreDto>
{
    public CreateMembreDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SiteId).GreaterThan(0).When(x => x.SiteId is not null);
    }
}
