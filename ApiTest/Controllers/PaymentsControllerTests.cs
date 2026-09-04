using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class PaymentsControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task PayParticipation_HappyPath_Returns200AndMarksParticipationPaid()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id);
        var participationId = await GetOrganizerParticipationIdAsync(match.Id, organizer);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/payments")
            { Content = JsonContent.Create(new PayDto("CB")) }.WithMember(organizer);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentDto>();
        Assert.Equal(15m, payment!.Amount);
        Assert.Equal("Validated", payment.Status);

        var participantsResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{match.Id}/participations").WithMember(organizer));
        var participants = await participantsResponse.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        Assert.Equal("Paid", participants!.Single().Status);
    }

    [Fact]
    public async Task PayParticipation_CallerDoesNotOwnParticipation_Returns404()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id);
        var participationId = await GetOrganizerParticipationIdAsync(match.Id, organizer);
        var stranger = await RegisterMemberAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/payments")
            { Content = JsonContent.Create(new PayDto()) }.WithMember(stranger);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PayParticipation_AlreadyPaid_Returns409()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id);
        var participationId = await GetOrganizerParticipationIdAsync(match.Id, organizer);

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/payments")
            { Content = JsonContent.Create(new PayDto()) }.WithMember(organizer))).EnsureSuccessStatusCode();

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/payments")
            { Content = JsonContent.Create(new PayDto()) }.WithMember(organizer));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetMyBalances_NoBalances_ReturnsEmptyList()
    {
        var matricule = await RegisterMemberAsync('G');

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/balances/me").WithMember(matricule));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<BalanceDueDto>>())!);
    }

    [Fact]
    public async Task GetMyBalances_ThenPayBalanceDue_SettlesTheBalance()
    {
        // An incomplete public match at J-1 creates a balance due for the organizer (RG-PUB-006/RG-PAY-005).
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        await CreateReservationAsync(organizer, court.Id, isPublic: true);

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/admin/daily-batch")
            .WithAdmin(NextAdminId()))).EnsureSuccessStatusCode();

        var balancesResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/balances/me").WithMember(organizer));
        var balances = await balancesResponse.Content.ReadFromJsonAsync<List<BalanceDueDto>>();
        Assert.Single(balances!);
        var balance = balances![0];

        var payResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/balances/{balance.Id}/payments")
            { Content = JsonContent.Create(new PayDto("CB")) }.WithMember(organizer));
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
        Assert.Equal(balance.Amount, (await payResponse.Content.ReadFromJsonAsync<PaymentDto>())!.Amount);

        var afterResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/balances/me").WithMember(organizer));
        Assert.Empty((await afterResponse.Content.ReadFromJsonAsync<List<BalanceDueDto>>())!);
    }

    [Fact]
    public async Task PayParticipation_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/participations/1/payments", new PayDto());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<int> GetOrganizerParticipationIdAsync(int matchId, string matricule)
    {
        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{matchId}/participations").WithMember(matricule));
        response.EnsureSuccessStatusCode();
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        return participants!.Single(p => p.Role == "Organizer").Id;
    }

    private async Task<string> RegisterMemberAsync(char prefix)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/members")
            { Content = JsonContent.Create(new CreateMemberDto("Doe", "Jane", MemberTestHelpers.TypeFromPrefix(prefix), null)) });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MemberDto>())!.Matricule;
    }

    private async Task<CourtDto> CreateCourtAsync()
    {
        var adminId = NextAdminId();
        var siteResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto("Site A", "Addr")) }.WithAdmin(adminId));
        siteResponse.EnsureSuccessStatusCode();
        var site = (await siteResponse.Content.ReadFromJsonAsync<SiteDto>())!;

        var courtResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/courts")
            { Content = JsonContent.Create(new CreateCourtDto("Court 1", site.Id)) }.WithAdmin(adminId));
        courtResponse.EnsureSuccessStatusCode();

        var scheduleResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/schedules")
            {
                Content = JsonContent.Create(new CreateSiteScheduleDto(Tomorrow.Year, SlotA, new TimeOnly(21, 0)))
            }.WithAdmin(adminId));
        scheduleResponse.EnsureSuccessStatusCode();

        return (await courtResponse.Content.ReadFromJsonAsync<CourtDto>())!;
    }

    private async Task<MatchDto> CreateReservationAsync(string matricule, int courtId, bool isPublic = false)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(courtId, Tomorrow, SlotA, isPublic)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
