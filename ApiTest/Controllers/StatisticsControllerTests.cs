using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class StatisticsControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static string NextMatricule(char prefix) => $"{prefix}{Random.Shared.Next(10_000, 99_999)}";
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly DateOnly Start = Tomorrow.AddDays(-1);
    private static readonly DateOnly End = Tomorrow.AddDays(1);
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task GetRevenue_SpecificSite_SumsOnlyValidatedPaymentsForThatSite()
    {
        var adminId = NextAdminId();
        var (site, court) = await CreateSiteAndCourtAsync(adminId);
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id);
        await PayOwnSeatAsync(match.Id, organizer);

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistics/revenue?siteId={site.Id}&start={Start:O}&end={End:O}").WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RevenueDto>();
        Assert.Equal(15m, result!.Amount);
    }

    [Fact]
    public async Task GetRevenue_SiteOwnedByAnotherAdmin_Returns404()
    {
        var siteOwner = NextAdminId();
        var (site, _) = await CreateSiteAndCourtAsync(siteOwner);

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistics/revenue?siteId={site.Id}&start={Start:O}&end={End:O}").WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRevenue_NoSiteId_AggregatesAcrossAllSitesOwnedByAdmin()
    {
        var adminId = NextAdminId();
        var (_, courtA) = await CreateSiteAndCourtAsync(adminId);
        var (_, courtB) = await CreateSiteAndCourtAsync(adminId);
        var organizerA = await RegisterMemberAsync('G');
        var matchA = await CreateReservationAsync(organizerA, courtA.Id);
        await PayOwnSeatAsync(matchA.Id, organizerA);
        var organizerB = await RegisterMemberAsync('L');
        var matchB = await CreateReservationAsync(organizerB, courtB.Id);
        await PayOwnSeatAsync(matchB.Id, organizerB);

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistics/revenue?start={Start:O}&end={End:O}").WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RevenueDto>();
        Assert.Equal(30m, result!.Amount);
    }

    [Fact]
    public async Task GetRevenue_MemberRoleInstead_Returns403()
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistics/revenue?start={Start:O}&end={End:O}").WithMember(NextMatricule('G')));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task PayOwnSeatAsync(int matchId, string matricule)
    {
        var participantsResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{matchId}/participations").WithMember(matricule));
        var participants = await participantsResponse.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        var participationId = participants!.Single(p => p.Role == "Organizer").Id;

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/payments")
            { Content = JsonContent.Create(new PayDto("CB")) }.WithMember(matricule))).EnsureSuccessStatusCode();
    }

    private async Task<string> RegisterMemberAsync(char prefix)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/members")
            { Content = JsonContent.Create(new CreateMemberDto("Doe", "Jane", MemberTestHelpers.TypeFromPrefix(prefix), null)) });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MemberDto>())!.Matricule;
    }

    private async Task<(SiteDto Site, CourtDto Court)> CreateSiteAndCourtAsync(int adminId)
    {
        var siteResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto($"Site {Guid.NewGuid():N}", "Addr")) }.WithAdmin(adminId));
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

        return (site, (await courtResponse.Content.ReadFromJsonAsync<CourtDto>())!);
    }

    private async Task<MatchDto> CreateReservationAsync(string matricule, int courtId)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(courtId, Tomorrow, SlotA)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
