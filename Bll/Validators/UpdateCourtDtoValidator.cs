using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class UpdateCourtDtoValidator : AbstractValidator<UpdateCourtDto>
{
    public UpdateCourtDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Le nom est obligatoire.")
            .MaximumLength(100)
            .WithMessage("Le nom ne peut pas dépasser 100 caractères.");
        RuleFor(x => x.Number).MaximumLength(20)
            .WithMessage("Le numéro ne peut pas dépasser 20 caractères.");
        RuleFor(x => x.SurfaceType).MaximumLength(50)
            .WithMessage("Le type de surface ne peut pas dépasser 50 caractères.");
    }
}
