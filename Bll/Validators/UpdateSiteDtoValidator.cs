using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class UpdateSiteDtoValidator : AbstractValidator<UpdateSiteDto>
{
    public UpdateSiteDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Le nom est obligatoire.")
            .MaximumLength(100)
            .WithMessage("Le nom ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.Address).NotEmpty()
            .WithMessage("L'adresse est obligatoire.")
            .MaximumLength(255)
            .WithMessage("L'adresse ne peut pas dépasser 255 caractères.");
        RuleFor(x => x.PostalCode).MaximumLength(20)
            .WithMessage("Le code postal ne peut pas dépasser 20 caractères.");
        RuleFor(x => x.City).MaximumLength(100)
            .WithMessage("La ville ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.Phone).MaximumLength(30)
            .WithMessage("Le numéro de téléphone ne peut pas dépasser 30 caractères.");
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress()
            .WithMessage("L'adresse email n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
