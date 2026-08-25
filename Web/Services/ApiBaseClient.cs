using Web.Services.Context;

namespace Web.Services;

public abstract class ApiBaseClient
{
    protected readonly HttpClient HttpClient;

    protected ApiBaseClient(HttpClient httpClient, UserContext userContext)
    {
        HttpClient = httpClient;
        HttpClient.DefaultRequestHeaders.Add("X-User-Role", userContext.Role.ToString());
        HttpClient.DefaultRequestHeaders.Add("X-User-Id", userContext.Id.ToString());
    }
}