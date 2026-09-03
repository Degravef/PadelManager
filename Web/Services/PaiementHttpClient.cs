using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class PaiementHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IPaiementService
{
    public async Task<PaiementDto> PayerParticipationAsync(int participationId, PayerDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/participations/{participationId}/paiements", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<PaiementDto>())!;
    }

    public async Task<PaiementDto> PayerSoldeAsync(int soldeDuId, PayerDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/soldes/{soldeDuId}/paiements", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<PaiementDto>())!;
    }

    public async Task<IEnumerable<SoldeDuDto>> GetMesSoldesAsync()
    {
        var response = await HttpClient.GetAsync("api/soldes/me");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<SoldeDuDto>>() ?? [];
    }
}
