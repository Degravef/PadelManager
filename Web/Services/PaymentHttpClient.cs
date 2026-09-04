using Web.Core.Dtos;
using Web.Core.Interfaces;
using Web.Services.Context;

namespace Web.Services;

public class PaymentHttpClient(HttpClient httpClient, UserContext userContext)
    : ApiBaseClient(httpClient, userContext), IPaymentService
{
    public async Task<PaymentDto> PayParticipationAsync(int participationId, PayDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/participations/{participationId}/payments", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<PaymentDto>())!;
    }

    public async Task<PaymentDto> PayBalanceDueAsync(int balanceDueId, PayDto dto)
    {
        var response = await HttpClient.PostAsJsonAsync($"api/balances/{balanceDueId}/payments", dto);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<PaymentDto>())!;
    }

    public async Task<IEnumerable<BalanceDueDto>> GetMyUnpaidBalancesAsync()
    {
        var response = await HttpClient.GetAsync("api/balances/me");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<BalanceDueDto>>() ?? [];
    }
}
