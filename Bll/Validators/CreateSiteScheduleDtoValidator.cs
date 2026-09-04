using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateSiteScheduleDtoValidator : AbstractValidator<CreateSiteScheduleDto>
{
    public CreateSiteScheduleDtoValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Year).GreaterThanOrEqualTo(_ => timeProvider.GetUtcNow().Year)
            .WithMessage("L'année ne peut pas être dans le passé.");
        RuleFor(x => x.ClosingTime).GreaterThan(x => x.OpeningTime)
            .WithMessage("L'heure de la dernière réservation doit être postérieure à l'heure de la première réservation.");
        RuleFor(x => x.MatchPrice).GreaterThan(0)
            .WithMessage("Le prix du match doit être supérieur à 0.")
            .When(x => x.MatchPrice is not null);
    }
}
