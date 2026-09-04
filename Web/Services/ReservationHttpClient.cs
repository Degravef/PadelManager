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

    public async Task<IEnumerable<SiteDto>> GetSitesAsync()
    {
        var response = await HttpClient.GetAsync("api/reservations/lookup/sites");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<SiteDto>>() ?? [];
    }

    public async Task<IEnumerable<TerrainDto>> GetTerrainsBySiteAsync(int siteId)
    {
        var response = await HttpClient.GetAsync($"api/reservations/lookup/sites/{siteId}/terrains");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<TerrainDto>>() ?? [];
    }

    public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int siteId, DateOnly date)
    {
        var response = await HttpClient.GetAsync($"api/reservations/lookup/sites/{siteId}/slots?date={date:yyyy-MM-dd}");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<AvailableSlotDto>>() ?? [];
    }
}
