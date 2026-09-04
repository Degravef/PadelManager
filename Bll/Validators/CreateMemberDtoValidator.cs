using Core.Constants;
using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    private static readonly string[] ValidMemberTypes =
        [MemberTypeSeed.GlobalCode, MemberTypeSeed.SiteCode, MemberTypeSeed.LibreCode];

    public CreateMemberDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Le nom est obligatoire.")
            .MaximumLength(100)
            .WithMessage("Le nom ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.FirstName).NotEmpty()
            .WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100)
            .WithMessage("Le prénom ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.Type).Must(t => ValidMemberTypes.Contains(t))
            .WithMessage($"Le type de membre doit être {string.Join(", ", ValidMemberTypes)}.");
        RuleFor(x => x.SiteId).GreaterThan(0)
            .WithMessage("L'identifiant du site doit être supérieur à 0.")
            .When(x => x.SiteId is not null);
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress()
            .WithMessage("L'adresse email n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30)
            .WithMessage("Le numéro de téléphone ne peut pas dépasser 30 caractères.");
    }
}
