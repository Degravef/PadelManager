using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateHoraireSiteDtoValidator : AbstractValidator<CreateHoraireSiteDto>
{
    public CreateHoraireSiteDtoValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Annee).GreaterThanOrEqualTo(_ => timeProvider.GetUtcNow().Year)
            .WithMessage("L'année ne peut pas être dans le passé.");
        RuleFor(x => x.HeureDerniereReservation).GreaterThan(x => x.HeurePremiereReservation)
            .WithMessage("L'heure de la dernière réservation doit être postérieure à l'heure de la première réservation.");
        RuleFor(x => x.PrixMatch).GreaterThan(0)
            .WithMessage("Le prix du match doit être supérieur à 0.")
            .When(x => x.PrixMatch is not null);
    }
}
