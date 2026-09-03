using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreerReservationDtoValidator : AbstractValidator<CreerReservationDto>
{
    public CreerReservationDtoValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.TerrainId).GreaterThan(0);

        // RG-RES-008: la date et l'heure du match doivent être postérieures à l'instant de la réservation.
        RuleFor(x => x)
            .Must(x => x.Date.ToDateTime(x.StartTime) > timeProvider.GetUtcNow().UtcDateTime)
            .WithName(nameof(CreerReservationDto.Date))
            .WithMessage("La date et l'heure de la réservation doivent être postérieures à l'instant présent.");
    }
}
