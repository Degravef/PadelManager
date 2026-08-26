using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class TerrainHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), ITerrainService
{
    public async Task<TerrainDto> GetTerrainByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/terrains/{id}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TerrainDto>())!;
    }

    public async Task<IEnumerable<TerrainDto>> GetAllTerrainsAsync(int? siteId = null)
    {
        var url = siteId is null ? "api/terrains" : $"api/terrains?siteId={siteId}";
        var response = await HttpClient.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<TerrainDto>>() ?? [];
    }

    public async Task<TerrainDto> CreateTerrainAsync(CreateTerrainDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/terrains", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TerrainDto>())!;
    }

    public async Task UpdateTerrainAsync(int id, UpdateTerrainDto dto)
    {
        var response = await HttpClient.PutAsJsonAsync($"api/terrains/{id}", dto);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteTerrainAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/terrains/{id}");
        await EnsureSuccessAsync(response);
    }
}
