using System.Net.Http.Json;
using Core.Dtos;
using Core.Interfaces.Services;

namespace Web.Services;

public class SiteHttpClient(HttpClient httpClient) : ISiteService
{
    public async Task<SiteDto?> GetSiteByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<SiteDto>($"api/sites/{id}");
    }

    public async Task<IEnumerable<SiteDto>> GetAllSitesAsync()
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<SiteDto>>("api/sites") ?? [];
    }

    public async Task<SiteDto> CreateSiteAsync(CreateSiteDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/sites", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    public async Task UpdateSiteAsync(int id, UpdateSiteDto dto)
    {
        var response = await httpClient.PutAsJsonAsync($"api/sites/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSiteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/sites/{id}");
        response.EnsureSuccessStatusCode();
    }
}
