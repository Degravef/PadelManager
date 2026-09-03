using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class HorairesSitesControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);
    private static readonly int NextYear = DateTime.UtcNow.Year + 1;

    [Fact]
    public async Task Create_ValidDto_Returns201WithLocationHeader()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/horaires")
            { Content = JsonContent.Create(new CreateHoraireSiteDto(NextYear, new TimeOnly(8, 0), new TimeOnly(21, 0))) }
            .WithAdmin(adminId);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var horaire = await response.Content.ReadFromJsonAsync<HoraireSiteDto>();
        Assert.Equal(site.Id, horaire!.SiteId);
        Assert.Equal(NextYear, horaire.Annee);
        Assert.Equal(60m, horaire.PrixMatch);
    }

    [Fact]
    public async Task Create_SiteOwnedByAnotherAdmin_Returns404()
    {
        var siteOwner = NextAdminId();
        var site = await CreateSiteAsync(siteOwner);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/horaires")
            { Content = JsonContent.Create(new CreateHoraireSiteDto(NextYear, new TimeOnly(8, 0), new TimeOnly(21, 0))) }
            .WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateYearForSameSite_Returns409()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId);
        var dto = new CreateHoraireSiteDto(NextYear, new TimeOnly(8, 0), new TimeOnly(21, 0));

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/horaires")
            { Content = JsonContent.Create(dto) }.WithAdmin(adminId))).EnsureSuccessStatusCode();

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/horaires")
            { Content = JsonContent.Create(dto) }.WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ClosingTimeBeforeOpeningTime_Returns400()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/sites/{site.Id}/horaires")
            { Content = JsonContent.Create(new CreateHoraireSiteDto(NextYear, new TimeOnly(21, 0), new TimeOnly(8, 0))) }
            .WithAdmin(adminId);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByYear_ExistingHoraire_ReturnsDto()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId);
        await CreateHoraireAsync(adminId, site.Id, NextYear);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/sites/{site.Id}/horaires/{NextYear}").WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(NextYear, (await response.Content.ReadFromJsonAsync<HoraireSiteDto>())!.Annee);
    }

    [Fact]
    public async Task GetByYear_NoHoraireForYear_Returns404()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/sites/{site.Id}/horaires/{NextYear}").WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/sites/1/horaires", new CreateHoraireSiteDto(NextYear, new TimeOnly(8, 0), new TimeOnly(21, 0)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<SiteDto> CreateSiteAsync(int adminId)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto("Site A", "Addr")) }.WithAdmin(adminId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    private async Task<HoraireSiteDto> CreateHoraireAsync(int adminId, int siteId, int annee)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/sites/{siteId}/horaires")
            { Content = JsonContent.Create(new CreateHoraireSiteDto(annee, new TimeOnly(8, 0), new TimeOnly(21, 0))) }.WithAdmin(adminId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HoraireSiteDto>())!;
    }
}
