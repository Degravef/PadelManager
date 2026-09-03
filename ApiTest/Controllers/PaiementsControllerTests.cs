using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class PaiementsControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task PayerParticipation_HappyPath_Returns200AndMarksParticipationPayee()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id);
        var participationId = await GetOrganisateurParticipationIdAsync(match.Id, organisateur);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/paiements")
            { Content = JsonContent.Create(new PayerDto("CB")) }.WithMember(organisateur);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paiement = await response.Content.ReadFromJsonAsync<PaiementDto>();
        Assert.Equal(15m, paiement!.Montant);
        Assert.Equal("Valide", paiement.Statut);

        var participantsResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{match.Id}/participations").WithMember(organisateur));
        var participants = await participantsResponse.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        Assert.Equal("Payee", participants!.Single().Statut);
    }

    [Fact]
    public async Task PayerParticipation_CallerDoesNotOwnParticipation_Returns404()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id);
        var participationId = await GetOrganisateurParticipationIdAsync(match.Id, organisateur);
        var stranger = await RegisterMembreAsync('L');

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/paiements")
            { Content = JsonContent.Create(new PayerDto()) }.WithMember(stranger);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PayerParticipation_AlreadyPaid_Returns409()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        var match = await CreateReservationAsync(organisateur, terrain.Id);
        var participationId = await GetOrganisateurParticipationIdAsync(match.Id, organisateur);

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/paiements")
            { Content = JsonContent.Create(new PayerDto()) }.WithMember(organisateur))).EnsureSuccessStatusCode();

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/paiements")
            { Content = JsonContent.Create(new PayerDto()) }.WithMember(organisateur));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetMesSoldes_NoSoldes_ReturnsEmptyList()
    {
        var matricule = await RegisterMembreAsync('G');

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/soldes/me").WithMember(matricule));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<SoldeDuDto>>())!);
    }

    [Fact]
    public async Task GetMesSoldes_ThenPayerSolde_SettlesTheBalance()
    {
        
        var terrain = await CreateTerrainAsync();
        var organisateur = await RegisterMembreAsync('G');
        await CreateReservationAsync(organisateur, terrain.Id, estPublic: true);

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/admin/traitement-quotidien")
            .WithAdmin(NextAdminId()))).EnsureSuccessStatusCode();

        var soldesResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/soldes/me").WithMember(organisateur));
        var soldes = await soldesResponse.Content.ReadFromJsonAsync<List<SoldeDuDto>>();
        Assert.Single(soldes!);
        var solde = soldes![0];

        var payResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/soldes/{solde.Id}/paiements")
            { Content = JsonContent.Create(new PayerDto("CB")) }.WithMember(organisateur));
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
        Assert.Equal(solde.Montant, (await payResponse.Content.ReadFromJsonAsync<PaiementDto>())!.Montant);

        var afterResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/soldes/me").WithMember(organisateur));
        Assert.Empty((await afterResponse.Content.ReadFromJsonAsync<List<SoldeDuDto>>())!);
    }

    [Fact]
    public async Task PayerParticipation_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/participations/1/paiements", new PayerDto());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<int> GetOrganisateurParticipationIdAsync(int matchId, string matricule)
    {
        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{matchId}/participations").WithMember(matricule));
        response.EnsureSuccessStatusCode();
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        return participants!.Single(p => p.Role == "Organisateur").Id;
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

    private async Task<MatchDto> CreateReservationAsync(string matricule, int terrainId, bool estPublic = false)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrainId, Tomorrow, SlotA, estPublic)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
