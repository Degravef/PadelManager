namespace Web.Services.Context;

public class UserContextHandler(UserContext userContext) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-User-Role", userContext.Role.ToString());
        request.Headers.Add("X-User-Id", userContext.Id);
        return base.SendAsync(request, cancellationToken);
    }
}