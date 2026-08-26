using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Handlers;

public class HeaderHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? role = Request.Headers["X-User-Role"].FirstOrDefault();
        string? userId = Request.Headers["X-User-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            return Task.FromResult(AuthenticateResult.Fail("Headers manquants"));

        // X-User-Id's format depends on the role (numeric admin id vs. matricule) and this handler
        // can't know which shape to expect — format validation happens per-role in ContextExtensions.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}