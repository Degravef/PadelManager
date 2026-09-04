using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class UpdateTerrainDtoValidator : AbstractValidator<UpdateTerrainDto>
{
    public UpdateTerrainDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Numero).MaximumLength(20);
        RuleFor(x => x.TypeSurface).MaximumLength(50);
    }
}
