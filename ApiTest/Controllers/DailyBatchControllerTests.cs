using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class DailyBatchControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static string NextMatricule(char prefix) => $"{prefix}{Random.Shared.Next(10_000, 99_999)}";
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task Execute_PrivateMatchIncompleteRoster_SwitchesToPublicAndPenalizesOrganizer()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "api/admin/daily-batch").WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DailyBatchResultDto>();
        Assert.Equal(1, result!.MatchesSwitchedIncompleteRoster);
        Assert.Equal(1, result.PenaltiesApplied);

        var getResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{match.Id}").WithMember(organizer));
        Assert.Equal("Public", (await getResponse.Content.ReadFromJsonAsync<MatchDto>())!.Type);
    }

    [Fact]
    public async Task Execute_ExplicitDateWithNoMatchesTheDayAfter_ReturnsZeroedResult()
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(
            HttpMethod.Post, $"api/admin/daily-batch?date={new DateOnly(2020, 1, 1):O}").WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DailyBatchResultDto>();
        Assert.Equal(0, result!.MatchesSwitchedIncompleteRoster);
        Assert.Equal(new DateOnly(2020, 1, 2), result.ProcessedDate);
    }

    [Fact]
    public async Task Execute_MemberRoleInstead_Returns403()
    {
        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "api/admin/daily-batch").WithMember(NextMatricule('G')));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Execute_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsync("api/admin/daily-batch", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    private async Task<MatchDto> CreateReservationAsync(string matricule, int courtId)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(courtId, Tomorrow, SlotA)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
