using System.Security.Claims;

namespace Api.Extensions;

public static class ContextExtensions
{
    public static int GetAdminId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}