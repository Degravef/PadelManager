using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class TraitementQuotidienControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static string NextMatricule(char prefix) => $"{prefix}{Random.Shared.Next(10_000, 99_999)}";
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly TimeOnly SlotA = new(8, 0);

    [Fact]
    public async Task Executer_PrivateMatchIncompleteRoster_SwitchesToPublicAndPenalizesOrganizer()
    {
        var terrain = await CreateTerrainAsync();
        var organisateur = NextMatricule('G');
        await RegisterMembreAsync(organisateur);
        var match = await CreateReservationAsync(organisateur, terrain.Id);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "api/admin/traitement-quotidien").WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TraitementQuotidienResultDto>();
        Assert.Equal(1, result!.MatchesBasculesEffectifIncomplet);
        Assert.Equal(1, result.PenalitesAppliquees);

        var getResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/matches/{match.Id}").WithMember(organisateur));
        Assert.Equal("Public", (await getResponse.Content.ReadFromJsonAsync<MatchDto>())!.TypeMatch);
    }

    [Fact]
    public async Task Executer_ExplicitDateWithNoMatchesTheDayAfter_ReturnsZeroedResult()
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(
            HttpMethod.Post, $"api/admin/traitement-quotidien?date={new DateOnly(2020, 1, 1):O}").WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TraitementQuotidienResultDto>();
        Assert.Equal(0, result!.MatchesBasculesEffectifIncomplet);
        Assert.Equal(new DateOnly(2020, 1, 2), result.DateTraitee);
    }

    [Fact]
    public async Task Executer_MemberRoleInstead_Returns403()
    {
        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "api/admin/traitement-quotidien").WithMember(NextMatricule('G')));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Executer_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsync("api/admin/traitement-quotidien", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task RegisterMembreAsync(string matricule)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
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

    private async Task<MatchDto> CreateReservationAsync(string matricule, int terrainId)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/matches")
            { Content = JsonContent.Create(new CreerReservationDto(terrainId, Tomorrow, SlotA)) }.WithMember(matricule));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
