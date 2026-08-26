using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class UpdateTerrainDtoValidator : AbstractValidator<UpdateTerrainDto>
{
    public UpdateTerrainDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
