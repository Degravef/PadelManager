using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class IdentityHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IIdentityService
{
    public async Task<IEnumerable<int>> GetAdminIdsAsync()
    {
        var response = await HttpClient.GetAsync("api/identity/admin-ids");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<int>>() ?? [];
    }

    public async Task<IEnumerable<string>> GetMatriculesAsync()
    {
        var response = await HttpClient.GetAsync("api/identity/matricules");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<string>>() ?? [];
    }
}
