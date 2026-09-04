using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class ParticipationsControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task AddPlayer_HappyPath_Returns201WithLocationHeader()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: false);
        var player = await RegisterMemberAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations")
            { Content = JsonContent.Create(new AddPlayerDto(player)) }.WithMember(organizer);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var participation = await response.Content.ReadFromJsonAsync<ParticipationDto>();
        Assert.Equal(match.Id, participation!.MatchId);
        Assert.Equal("Player", participation.Role);
    }

    [Fact]
    public async Task AddPlayer_MatchIsPublic_Returns400()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: true);
        var player = await RegisterMemberAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations")
            { Content = JsonContent.Create(new AddPlayerDto(player)) }.WithMember(organizer);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddPlayer_CallerNotOrganizer_Returns404()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: false);
        var stranger = await RegisterMemberAsync('L');
        var player = await RegisterMemberAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations")
            { Content = JsonContent.Create(new AddPlayerDto(player)) }.WithMember(stranger);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Join_PublicMatch_HappyPath_Returns201()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: true);
        var player = await RegisterMemberAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join").WithMember(player);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var participation = await response.Content.ReadFromJsonAsync<ParticipationDto>();
        Assert.Equal("Reserved", participation!.Status);
    }

    [Fact]
    public async Task Join_ConcurrentRaceForLastSeat_ExactlyOneSucceeds()
    {
        // RG-PUB-002/003/004 under real concurrency ("first paid, first served" only holds up if
        // seat registration itself can't double-book): fills seats 1-3, then two members race for
        // the last seat at the same time. Proves UQ_Participations_MatchId_SeatNumber (plus the
        // CK_Participations_SeatNumber_Range check) is what actually stops a 5th seat, not just the
        // app-level RosterCompleteRule read-then-check.
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G'); // seat 1
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: true);
        var secondPlayer = await RegisterMemberAsync('L'); // seat 2
        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join")
            .WithMember(secondPlayer))).EnsureSuccessStatusCode();
        var thirdPlayer = await RegisterMemberAsync('L'); // seat 3
        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join")
            .WithMember(thirdPlayer))).EnsureSuccessStatusCode();

        var fourthPlayer = await RegisterMemberAsync('L');
        var fifthPlayer = await RegisterMemberAsync('L');

        Task<HttpResponseMessage> first = fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join").WithMember(fourthPlayer));
        Task<HttpResponseMessage> second = fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join").WithMember(fifthPlayer));
        HttpResponseMessage[] responses = await Task.WhenAll(first, second);

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_PrivateMatch_Returns400()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: false);
        var player = await RegisterMemberAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join").WithMember(player);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetParticipants_ExistingMatch_ReturnsOrganizerAsFirstParticipant()
    {
        var court = await CreateCourtAsync();
        var organizer = await RegisterMemberAsync('G');
        var match = await CreateReservationAsync(organizer, court.Id, isPublic: false);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{match.Id}/participations").WithMember(organizer));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        Assert.Single(participants!);
        Assert.Equal("Organizer", participants![0].Role);
    }

    [Fact]
    public async Task AddPlayer_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/matches/1/participations", new AddPlayerDto("G1"));

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

    private async Task<MatchDto> CreateReservationAsync(string matricule, int courtId, bool isPublic)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreateReservationDto(courtId, Tomorrow, SlotA, isPublic)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
