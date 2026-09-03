using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class ParticipationHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IParticipationService
{
    public async Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId)
    {
        var response = await HttpClient.GetAsync($"api/matches/{matchId}/participations");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<ParticipationDto>>() ?? [];
    }

    public async Task<ParticipationDto> AjouterJoueurAsync(int matchId, AjouterJoueurDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/matches/{matchId}/participations", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<ParticipationDto>())!;
    }

    public async Task<ParticipationDto> RejoindreAsync(int matchId)
    {
        var response = await HttpClient.PostAsync($"api/matches/{matchId}/participations/join", null);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<ParticipationDto>())!;
    }
}
