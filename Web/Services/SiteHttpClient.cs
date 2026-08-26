using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class SiteHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), ISiteService
{
    public async Task<SiteDto> GetSiteByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/sites/{id}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    public async Task<IEnumerable<SiteDto>> GetAllSitesAsync()
    {
        var response = await HttpClient.GetAsync("api/sites");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<SiteDto>>() ?? [];
    }

    public async Task<SiteDto> CreateSiteAsync(CreateSiteDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/sites", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    public async Task UpdateSiteAsync(int id, UpdateSiteDto dto)
    {
        var response = await HttpClient.PutAsJsonAsync($"api/sites/{id}", dto);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteSiteAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/sites/{id}");
        await EnsureSuccessAsync(response);
    }
}