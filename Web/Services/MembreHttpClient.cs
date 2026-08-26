using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class MembreHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IMembreService
{
    public async Task<MembreDto> GetMembreByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/membres/{id}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MembreDto>())!;
    }

    public async Task<MembreDto> CreateMembreAsync(CreateMembreDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/membres", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MembreDto>())!;
    }
}
