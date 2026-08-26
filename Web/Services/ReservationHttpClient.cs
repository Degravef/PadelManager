using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class ReservationHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IReservationService
{
    public async Task<MatchDto> GetReservationByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/matches/{id}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }

    public async Task<IEnumerable<MatchDto>> GetMyReservationsAsync()
    {
        var response = await HttpClient.GetAsync("api/matches/me");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<MatchDto>>() ?? [];
    }

    public async Task<MatchDto> CreerReservationAsync(CreerReservationDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/matches", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
