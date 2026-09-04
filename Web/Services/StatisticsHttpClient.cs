using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class StatisticsHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IStatisticsService
{
    public async Task<RevenueDto> CalculateRevenueAsync(int? siteId, DateOnly start, DateOnly end)
    {
        var query = $"start={start:O}&end={end:O}" + (siteId is null ? "" : $"&siteId={siteId}");
        var response = await HttpClient.GetAsync($"api/statistics/revenue?{query}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<RevenueDto>())!;
    }
}
