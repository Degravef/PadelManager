using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class MatchLifecycleHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IMatchLifecycleService
{
    public async Task<TraitementQuotidienResultDto> ExecuterAsync(DateOnly? date)
    {
        var query = date is null ? "" : $"?date={date:O}";
        var response = await HttpClient.PostAsync($"api/admin/traitement-quotidien{query}", null);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TraitementQuotidienResultDto>())!;
    }
}
