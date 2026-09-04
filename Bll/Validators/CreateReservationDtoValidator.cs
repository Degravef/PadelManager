using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.CourtId).GreaterThan(0)
            .WithMessage("L'identifiant du terrain doit être supérieur à 0.");

        // RG-RES-008
        RuleFor(x => x)
            .Must(x => x.Date.ToDateTime(x.StartTime) > timeProvider.GetUtcNow().UtcDateTime)
            .WithName(nameof(CreateReservationDto.Date))
            .WithMessage("La date et l'heure de la réservation doivent être postérieures à l'instant présent.");
    }
}
