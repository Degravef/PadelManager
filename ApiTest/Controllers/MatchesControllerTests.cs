using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class MatchesControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static string NextMatricule(char prefix) => $"{prefix}{Random.Shared.Next(10_000, 99_999)}";
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    // CreateCourtAsync seeds a SiteSchedule opening at 08:00 with 90-minute matches + 15-minute buffer
    // (RG-SITE-003/004) — these are two of the generated 1h45-spaced slots (08:00, 09:45, 11:30, ...).
    private static readonly TimeOnly SlotA = new(8, 0);
    private static readonly TimeOnly SlotB = new(9, 45);

    [Fact]
    public async Task Create_ValidDto_Returns201WithLocationHeader()
    {
        var matricule = await RegisterMemberAsync('G');
        var court = await CreateCourtAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(court.Id, Tomorrow, SlotA)) }
            .WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var match = await response.Content.ReadFromJsonAsync<MatchDto>();
        Assert.Equal(court.Id, match!.CourtId);
        Assert.Equal("Private", match.Type);
        Assert.Equal("Open", match.Status);
        Assert.Equal(60m, match.TotalAmount);
    }

    [Fact]
    public async Task Create_UnknownCourt_Returns404()
    {
        var matricule = await RegisterMemberAsync('G');

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(999_999, Tomorrow, SlotA)) }
            .WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnregisteredMatricule_Returns404()
    {
        var court = await CreateCourtAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(court.Id, Tomorrow, SlotA)) }
            .WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_DateInThePast_Returns400()
    {
        var matricule = await RegisterMemberAsync('G');
        var court = await CreateCourtAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            {
                Content = JsonContent.Create(new CreateReservationDto(
                    court.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), SlotA))
            }.WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ExactDoubleBookingSameCourtDateAndTime_Returns409()
    {
        var court = await CreateCourtAsync();
        var dto = new CreateReservationDto(court.Id, Tomorrow, SlotA);

        var firstMatricule = await RegisterMemberAsync('G');
        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(dto) }.WithMember(firstMatricule))).EnsureSuccessStatusCode();

        var secondMatricule = await RegisterMemberAsync('L');
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(dto) }.WithMember(secondMatricule));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ConcurrentDoubleBookingSameCourtDateAndTime_ExactlyOneSucceeds()
    {
        // RG-SITE-006 under real concurrency: two requests hit the API at the same time, so the
        // app-level SlotAvailableRule check alone can't serialize them — this proves the DB's
        // partial unique index (UQ_Matches_CourtId_Date_StartTime) is the actual source of truth.
        var court = await CreateCourtAsync();
        var dto = new CreateReservationDto(court.Id, Tomorrow, SlotA);
        var firstMatricule = await RegisterMemberAsync('G');
        var secondMatricule = await RegisterMemberAsync('L');

        Task<HttpResponseMessage> first = fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(dto) }.WithMember(firstMatricule));
        Task<HttpResponseMessage> second = fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(dto) }.WithMember(secondMatricule));
        HttpResponseMessage[] responses = await Task.WhenAll(first, second);

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_DifferentStartTimeSameCourtAndDate_BothSucceed()
    {
        var court = await CreateCourtAsync();

        var firstMatricule = await RegisterMemberAsync('G');
        var first = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(court.Id, Tomorrow, SlotA)) }
            .WithMember(firstMatricule));

        var secondMatricule = await RegisterMemberAsync('L');
        var second = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(court.Id, Tomorrow, SlotB)) }
            .WithMember(secondMatricule));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingMatch_ViewedByOrganizer_ReturnsDto()
    {
        var matricule = await RegisterMemberAsync('G');
        var court = await CreateCourtAsync();
        var created = await CreateReservationAsync(matricule, court.Id, Tomorrow, SlotA);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{created.Id}").WithMember(matricule));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, (await response.Content.ReadFromJsonAsync<MatchDto>())!.Id);
    }

    [Fact]
    public async Task GetById_PrivateMatchViewedByNonParticipant_Returns404()
    {
        // RG-PRV-003: a private match is only visible to its own registered participants.
        var matricule = await RegisterMemberAsync('G');
        var court = await CreateCourtAsync();
        var created = await CreateReservationAsync(matricule, court.Id, Tomorrow, SlotA);

        var stranger = await RegisterMemberAsync('L');

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{created.Id}").WithMember(stranger));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/matches/999999").WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_ReturnsOnlyOwnReservations()
    {
        var matricule = await RegisterMemberAsync('G');
        var court = await CreateCourtAsync();
        var created = await CreateReservationAsync(matricule, court.Id, Tomorrow, SlotA);

        var otherMatricule = await RegisterMemberAsync('L');
        await CreateReservationAsync(otherMatricule, court.Id, Tomorrow, SlotB);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/matches/me").WithMember(matricule));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var matches = await response.Content.ReadFromJsonAsync<List<MatchDto>>();
        Assert.Single(matches!);
        Assert.Equal(created.Id, matches![0].Id);
    }

    [Fact]
    public async Task GetMine_UnregisteredMatricule_Returns404()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/matches/me").WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/matches", new CreateReservationDto(1, Tomorrow, SlotA));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AdminRoleInstead_Returns403()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(1, Tomorrow, SlotA)) }
            .WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

        // RG-SITE-002: real slot calculation requires an opening-hours row for the site/year.
        var scheduleResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/schedules")
            {
                Content = JsonContent.Create(new CreateSiteScheduleDto(Tomorrow.Year, SlotA, new TimeOnly(21, 0)))
            }.WithAdmin(adminId));
        scheduleResponse.EnsureSuccessStatusCode();

        return (await courtResponse.Content.ReadFromJsonAsync<CourtDto>())!;
    }

    private async Task<MatchDto> CreateReservationAsync(string matricule, int courtId, DateOnly date, TimeOnly startTime)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(courtId, date, startTime)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
