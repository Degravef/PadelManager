using Core.Dtos;
using FluentValidation;

namespace Bll.Validators;

public class UpdateSiteDtoValidator : AbstractValidator<UpdateSiteDto>
{
    public UpdateSiteDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(255);
    }
}