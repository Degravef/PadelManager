using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class MatchLifecycleHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IMatchLifecycleService
{
    public async Task<DailyBatchResultDto> ExecuteDailyBatchAsync(DateOnly? today)
    {
        var query = today is null ? "" : $"?date={today:O}";
        var response = await HttpClient.PostAsync($"api/admin/daily-batch{query}", null);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<DailyBatchResultDto>())!;
    }
}
