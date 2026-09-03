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
    public async Task AjouterJoueur_HappyPath_Returns201WithLocationHeader()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id, estPublic: false);
        var joueur = await RegisterMembreAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations")
            { Content = JsonContent.Create(new AjouterJoueurDto(joueur)) }.WithMember(organisateur);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var participation = await response.Content.ReadFromJsonAsync<ParticipationDto>();
        Assert.Equal(match.Id, participation!.MatchId);
        Assert.Equal("Joueur", participation.Role);
    }

    [Fact]
    public async Task AjouterJoueur_MatchIsPublic_Returns400()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id, estPublic: true);
        var joueur = await RegisterMembreAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations")
            { Content = JsonContent.Create(new AjouterJoueurDto(joueur)) }.WithMember(organisateur);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AjouterJoueur_CallerNotOrganizer_Returns404()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id, estPublic: false);
        var stranger = await RegisterMembreAsync('L');
        var joueur = await RegisterMembreAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations")
            { Content = JsonContent.Create(new AjouterJoueurDto(joueur)) }.WithMember(stranger);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rejoindre_PublicMatch_HappyPath_Returns201()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id, estPublic: true);
        var joueur = await RegisterMembreAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join").WithMember(joueur);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var participation = await response.Content.ReadFromJsonAsync<ParticipationDto>();
        Assert.Equal("Reservee", participation!.Statut);
    }

    [Fact]
    public async Task Rejoindre_PrivateMatch_Returns400()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id, estPublic: false);
        var joueur = await RegisterMembreAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{match.Id}/participations/join").WithMember(joueur);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetParticipants_ExistingMatch_ReturnsOrganizerAsFirstParticipant()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id, estPublic: false);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{match.Id}/participations").WithMember(organisateur));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        Assert.Single(participants!);
        Assert.Equal("Organisateur", participants![0].Role);
    }

    [Fact]
    public async Task AjouterJoueur_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/matches/1/participations", new AjouterJoueurDto("G1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    private async Task<MatchDto> CreateReservationAsync(string matricule, int terrainId, bool estPublic)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrainId, Tomorrow, SlotA, estPublic)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
