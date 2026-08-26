using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class CreateSiteDtoValidator : AbstractValidator<CreateSiteDto>
{
    public CreateSiteDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(255);
    }
}