using System.Security.Claims;
using System.Text.RegularExpressions;
using Core.Domain.Exceptions;

namespace Api.Extensions;

public static partial class ContextExtensions
{
    [GeneratedRegex(@"^[GSL]\d{1,5}$")]
    private static partial Regex MatriculeRegex();

    public static int GetAdminId(this ClaimsPrincipal user)
    {
        string? value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value is null || !int.TryParse(value, out int adminId))
            throw new InvalidAdminIdException();
        return adminId;
    }

    public static string GetMatricule(this ClaimsPrincipal user)
    {
        string? value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value is null || !MatriculeRegex().IsMatch(value))
            throw new InvalidMatriculeException();
        return value;
    }
}