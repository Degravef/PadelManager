using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class SiteHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), ISiteService
{
    public async Task<SiteDto?> GetSiteByIdAsync(int id)
    {
        return await HttpClient.GetFromJsonAsync<SiteDto>($"api/sites/{id}");
    }

    public async Task<IEnumerable<SiteDto>> GetAllSitesAsync()
    {
        Console.WriteLine("getting all sites");
        return await HttpClient.GetFromJsonAsync<IEnumerable<SiteDto>>("api/sites") ?? [];
    }

    public async Task<SiteDto> CreateSiteAsync(CreateSiteDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/sites", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    public async Task UpdateSiteAsync(int id, UpdateSiteDto dto)
    {
        var response = await HttpClient.PutAsJsonAsync($"api/sites/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSiteAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/sites/{id}");
        response.EnsureSuccessStatusCode();
    }
}