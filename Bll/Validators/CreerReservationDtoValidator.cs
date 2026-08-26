using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreerReservationDtoValidator : AbstractValidator<CreerReservationDto>
{
    public CreerReservationDtoValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.TerrainId).GreaterThan(0);
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("La date de la réservation ne peut pas être dans le passé.");
    }
}
