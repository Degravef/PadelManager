using System.Security.Claims;

namespace Api.Middleware;

public class HeaderContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var role = context.Request.Headers["X-User-Role"].FirstOrDefault();
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();

        if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(userId))
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Role, role)
            };

            // "AuthenticationType" non-null est ce qui rend IsAuthenticated = true
            var identity = new ClaimsIdentity(claims, authenticationType: "HeaderAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        await next(context);
    }
}