using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class AddPlayerDtoValidator : AbstractValidator<AddPlayerDto>
{
    public AddPlayerDtoValidator()
    {
        RuleFor(x => x.Matricule).Matches("^[GSL]\\d{1,5}$")
            .WithMessage("Le matricule doit être au format G/S/L suivi de 1 à 5 chiffres.");
    }
}
