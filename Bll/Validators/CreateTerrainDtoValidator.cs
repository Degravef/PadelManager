using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateTerrainDtoValidator : AbstractValidator<CreateTerrainDto>
{
    public CreateTerrainDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SiteId).GreaterThan(0);
        RuleFor(x => x.Numero).MaximumLength(20);
        RuleFor(x => x.TypeSurface).MaximumLength(50);
    }
}
