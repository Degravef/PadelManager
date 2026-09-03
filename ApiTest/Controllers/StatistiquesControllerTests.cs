using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class StatistiquesControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static string NextMatricule(char prefix) => $"{prefix}{Random.Shared.Next(10_000, 99_999)}";
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly DateOnly Debut = Tomorrow.AddDays(-1);
    private static readonly DateOnly Fin = Tomorrow.AddDays(1);
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task GetChiffreAffaires_SpecificSite_SumsOnlyValidatedPaymentsForThatSite()
    {
        var adminId = NextAdminId();
        var (site, terrain) = await CreateSiteAndTerrainAsync(adminId);
        var organisateur = NextMatricule('G');
        await RegisterMembreAsync(organisateur);
        var match = await CreateReservationAsync(organisateur, terrain.Id);
        await PayOwnSeatAsync(match.Id, organisateur);

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistiques/chiffre-affaires?siteId={site.Id}&debut={Debut:O}&fin={Fin:O}").WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChiffreAffairesDto>();
        Assert.Equal(15m, result!.Montant);
    }

    [Fact]
    public async Task GetChiffreAffaires_SiteOwnedByAnotherAdmin_Returns404()
    {
        var siteOwner = NextAdminId();
        var (site, _) = await CreateSiteAndTerrainAsync(siteOwner);

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistiques/chiffre-affaires?siteId={site.Id}&debut={Debut:O}&fin={Fin:O}").WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetChiffreAffaires_NoSiteId_AggregatesAcrossAllSitesOwnedByAdmin()
    {
        var adminId = NextAdminId();
        var (_, terrainA) = await CreateSiteAndTerrainAsync(adminId);
        var (_, terrainB) = await CreateSiteAndTerrainAsync(adminId);
        var organisateurA = NextMatricule('G');
        await RegisterMembreAsync(organisateurA);
        var matchA = await CreateReservationAsync(organisateurA, terrainA.Id);
        await PayOwnSeatAsync(matchA.Id, organisateurA);
        var organisateurB = NextMatricule('L');
        await RegisterMembreAsync(organisateurB);
        var matchB = await CreateReservationAsync(organisateurB, terrainB.Id);
        await PayOwnSeatAsync(matchB.Id, organisateurB);

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistiques/chiffre-affaires?debut={Debut:O}&fin={Fin:O}").WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChiffreAffairesDto>();
        Assert.Equal(30m, result!.Montant);
    }

    [Fact]
    public async Task GetChiffreAffaires_MemberRoleInstead_Returns403()
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"api/statistiques/chiffre-affaires?debut={Debut:O}&fin={Fin:O}").WithMember(NextMatricule('G')));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task PayOwnSeatAsync(int matchId, string matricule)
    {
        var participantsResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{matchId}/participations").WithMember(matricule));
        var participants = await participantsResponse.Content.ReadFromJsonAsync<List<ParticipationDto>>();
        var participationId = participants!.Single(p => p.Role == "Organisateur").Id;

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/participations/{participationId}/paiements")
            { Content = JsonContent.Create(new PayerDto("CB")) }.WithMember(matricule))).EnsureSuccessStatusCode();
    }

    private async Task RegisterMembreAsync(string matricule)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
    }

    private async Task<(SiteDto Site, TerrainDto Terrain)> CreateSiteAndTerrainAsync(int adminId)
    {
        var siteResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto($"Site {Guid.NewGuid():N}", "Addr")) }.WithAdmin(adminId));
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

        return (site, (await terrainResponse.Content.ReadFromJsonAsync<TerrainDto>())!);
    }

    private async Task<MatchDto> CreateReservationAsync(string matricule, int terrainId)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrainId, Tomorrow, SlotA)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
