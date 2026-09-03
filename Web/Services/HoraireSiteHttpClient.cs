using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class HoraireSiteHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IHoraireSiteService
{
    public async Task<HoraireSiteDto> GetBySiteAndYearAsync(int siteId, int annee)
    {
        var response = await HttpClient.GetAsync($"api/sites/{siteId}/horaires/{annee}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<HoraireSiteDto>())!;
    }

    public async Task<HoraireSiteDto> CreateAsync(int siteId, CreateHoraireSiteDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/sites/{siteId}/horaires", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<HoraireSiteDto>())!;
    }
}
