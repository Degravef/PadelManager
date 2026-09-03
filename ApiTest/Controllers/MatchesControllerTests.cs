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

    
    
    private static readonly TimeOnly SlotA = new(8, 0);
    private static readonly TimeOnly SlotB = new(9, 45);

    [Fact]
    public async Task Create_ValidDto_Returns201WithLocationHeader()
    {
        var matricule = await RegisterMembreAsync('G');
        var terrain = await CreateTerrainAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrain.Id, Tomorrow, SlotA)) }
            .WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var match = await response.Content.ReadFromJsonAsync<MatchDto>();
        Assert.Equal(terrain.Id, match!.TerrainId);
        Assert.Equal("Private", match.TypeMatch);
        Assert.Equal("Open", match.Statut);
        Assert.Equal(60m, match.MontantTotal);
    }

    [Fact]
    public async Task Create_UnknownTerrain_Returns404()
    {
        var matricule = await RegisterMembreAsync('G');

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(999_999, Tomorrow, SlotA)) }
            .WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnregisteredMatricule_Returns404()
    {
        var terrain = await CreateTerrainAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrain.Id, Tomorrow, SlotA)) }
            .WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_DateInThePast_Returns400()
    {
        var matricule = await RegisterMembreAsync('G');
        var terrain = await CreateTerrainAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            {
                Content = JsonContent.Create(new CreerReservationDto(
                    terrain.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), SlotA))
            }.WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ExactDoubleBookingSameCourtDateAndTime_Returns409()
    {
        var terrain = await CreateTerrainAsync();
        var dto = new CreerReservationDto(terrain.Id, Tomorrow, SlotA);

        var firstMatricule = await RegisterMembreAsync('G');
        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(dto) }.WithMember(firstMatricule))).EnsureSuccessStatusCode();

        var secondMatricule = await RegisterMembreAsync('L');
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(dto) }.WithMember(secondMatricule));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_DifferentStartTimeSameCourtAndDate_BothSucceed()
    {
        var terrain = await CreateTerrainAsync();

        var firstMatricule = await RegisterMembreAsync('G');
        var first = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrain.Id, Tomorrow, SlotA)) }
            .WithMember(firstMatricule));

        var secondMatricule = await RegisterMembreAsync('L');
        var second = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrain.Id, Tomorrow, SlotB)) }
            .WithMember(secondMatricule));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingMatch_ViewedByOrganizer_ReturnsDto()
    {
        var matricule = await RegisterMembreAsync('G');
        var terrain = await CreateTerrainAsync();
        var created = await CreateReservationAsync(matricule, terrain.Id, Tomorrow, SlotA);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{created.Id}").WithMember(matricule));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, (await response.Content.ReadFromJsonAsync<MatchDto>())!.Id);
    }

    [Fact]
    public async Task GetById_PrivateMatchViewedByNonParticipant_Returns404()
    {
        
        var matricule = await RegisterMembreAsync('G');
        var terrain = await CreateTerrainAsync();
        var created = await CreateReservationAsync(matricule, terrain.Id, Tomorrow, SlotA);

        var stranger = await RegisterMembreAsync('L');

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
        var matricule = await RegisterMembreAsync('G');
        var terrain = await CreateTerrainAsync();
        var created = await CreateReservationAsync(matricule, terrain.Id, Tomorrow, SlotA);

        var otherMatricule = await RegisterMembreAsync('L');
        await CreateReservationAsync(otherMatricule, terrain.Id, Tomorrow, SlotB);

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
        var response = await fixture.Client.PostAsJsonAsync("api/matches", new CreerReservationDto(1, Tomorrow, SlotA));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AdminRoleInstead_Returns403()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(1, Tomorrow, SlotA)) }
            .WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> RegisterMembreAsync(char prefix)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", MembreTestHelpers.TypeFromPrefix(prefix), null)) });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MembreDto>())!.Matricule;
    }

    private async Task<TerrainDto> CreateTerrainAsync()
    {
        var adminId = NextAdminId();
        var siteResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto("Site A", "Addr")) }.WithAdmin(adminId));
        siteResponse.EnsureSuccessStatusCode();
        var site = (await siteResponse.Content.ReadFromJsonAsync<SiteDto>())!;

        var terrainResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto("Court 1", site.Id)) }.WithAdmin(adminId));
        terrainResponse.EnsureSuccessStatusCode();

        
        var horaireResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/horaires")
            {
                Content = JsonContent.Create(new CreateHoraireSiteDto(Tomorrow.Year, SlotA, new TimeOnly(21, 0)))
            }.WithAdmin(adminId));
        horaireResponse.EnsureSuccessStatusCode();

        return (await terrainResponse.Content.ReadFromJsonAsync<TerrainDto>())!;
    }

    private async Task<MatchDto> CreateReservationAsync(string matricule, int terrainId, DateOnly date, TimeOnly startTime)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrainId, date, startTime)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
