using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class CourtHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), ICourtService
{
    public async Task<CourtDto> GetCourtByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/courts/{id}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<CourtDto>())!;
    }

    public async Task<IEnumerable<CourtDto>> GetAllCourtsAsync(int? siteId = null)
    {
        var url = siteId is null ? "api/courts" : $"api/courts?siteId={siteId}";
        var response = await HttpClient.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<CourtDto>>() ?? [];
    }

    public async Task<CourtDto> CreateCourtAsync(CreateCourtDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/courts", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<CourtDto>())!;
    }

    public async Task UpdateCourtAsync(int id, UpdateCourtDto dto)
    {
        var response = await HttpClient.PutAsJsonAsync($"api/courts/{id}", dto);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteCourtAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/courts/{id}");
        await EnsureSuccessAsync(response);
    }
}
