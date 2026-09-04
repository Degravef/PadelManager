using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class SiteScheduleHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), ISiteScheduleService
{
    public async Task<SiteScheduleDto> GetBySiteAndYearAsync(int siteId, int year)
    {
        var response = await HttpClient.GetAsync($"api/sites/{siteId}/schedules/{year}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<SiteScheduleDto>())!;
    }

    public async Task<SiteScheduleDto> CreateAsync(int siteId, CreateSiteScheduleDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/sites/{siteId}/schedules", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<SiteScheduleDto>())!;
    }
}
