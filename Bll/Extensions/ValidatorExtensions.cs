using FluentValidation;
using FluentValidation.Results;

namespace Bll.Extensions;

internal static class ValidatorExtensions
{
    public static async Task ValidateOrThrowAsync<T>(this IValidator<T> validator, T instance, CancellationToken ct = default)
    {
        ValidationResult? result = await validator.ValidateAsync(instance, ct);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }
}