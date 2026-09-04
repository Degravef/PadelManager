using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class MemberHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IMemberService
{
    public async Task<MemberDto> GetMemberByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/members/{id}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MemberDto>())!;
    }

    public async Task<MemberDto> GetMyMemberAsync()
    {
        var response = await HttpClient.GetAsync("api/members/me");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MemberDto>())!;
    }

    public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
    {
        var response = await HttpClient.GetAsync("api/members");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<MemberDto>>() ?? [];
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/members", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MemberDto>())!;
    }
}
