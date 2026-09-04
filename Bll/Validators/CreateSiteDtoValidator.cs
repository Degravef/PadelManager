using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateSiteDtoValidator : AbstractValidator<CreateSiteDto>
{
    public CreateSiteDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress()
            .WithMessage("L'adresse email n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
