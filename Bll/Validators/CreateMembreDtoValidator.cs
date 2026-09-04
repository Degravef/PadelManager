using Core.Constants;
using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateMembreDtoValidator : AbstractValidator<CreateMembreDto>
{
    private static readonly string[] TypesMembreValides =
        [TypeMembreSeed.GlobalCode, TypeMembreSeed.SiteCode, TypeMembreSeed.LibreCode];

    public CreateMembreDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Le nom est obligatoire.")
            .MaximumLength(100)
            .WithMessage("Le nom ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.FirstName).NotEmpty()
            .WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100)
            .WithMessage("Le prénom ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.Type).Must(t => TypesMembreValides.Contains(t))
            .WithMessage($"Le type de membre doit être {string.Join(", ", TypesMembreValides)}.");
        RuleFor(x => x.SiteId).GreaterThan(0)
            .WithMessage("L'identifiant du site doit être supérieur à 0.")
            .When(x => x.SiteId is not null);
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress()
            .WithMessage("L'adresse email n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Telephone).MaximumLength(30)
            .WithMessage("Le numéro de téléphone ne peut pas dépasser 30 caractères.");
    }
}
