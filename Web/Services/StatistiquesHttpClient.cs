using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class StatistiquesHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IStatistiquesService
{
    public async Task<ChiffreAffairesDto> GetChiffreAffairesAsync(int? siteId, DateOnly debut, DateOnly fin)
    {
        var query = $"debut={debut:O}&fin={fin:O}" + (siteId is null ? "" : $"&siteId={siteId}");
        var response = await HttpClient.GetAsync($"api/statistiques/chiffre-affaires?{query}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<ChiffreAffairesDto>())!;
    }
}
