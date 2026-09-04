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

    public async Task<ParticipationDto> AddPlayerToPrivateMatchAsync(int matchId, AddPlayerDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/matches/{matchId}/participations", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<ParticipationDto>())!;
    }

    public async Task<ParticipationDto> JoinPublicMatchAsync(int matchId)
    {
        var response = await HttpClient.PostAsync($"api/matches/{matchId}/participations/join", null);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<ParticipationDto>())!;
    }
}
