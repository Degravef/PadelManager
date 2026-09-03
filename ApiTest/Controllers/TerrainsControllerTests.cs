using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class TerrainsControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);

    [Fact]
    public async Task GetAll_NoTerrainsForAdmin_ReturnsEmptyList()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/terrains").WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<TerrainDto>>())!);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201WithLocationHeader()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId, "Site A");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto("Court 1", site.Id)) }.WithAdmin(adminId);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var terrain = await response.Content.ReadFromJsonAsync<TerrainDto>();
        Assert.Equal("Court 1", terrain!.Name);
        Assert.Equal(site.Id, terrain.SiteId);
    }

    [Fact]
    public async Task Create_MissingName_Returns400()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId, "Site A");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto("", site.Id)) }.WithAdmin(adminId);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_SiteOwnedByAnotherAdmin_Returns404()
    {
        var siteOwner = NextAdminId();
        var site = await CreateSiteAsync(siteOwner, "Site A");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto("Court 1", site.Id)) }.WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateNameForSameSite_Returns409()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId, "Site A");
        var dto = new CreateTerrainDto("Duplicate Court", site.Id);

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(dto) }.WithAdmin(adminId))).EnsureSuccessStatusCode();

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(dto) }.WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_SameNameDifferentSites_BothSucceed()
    {
        var adminId = NextAdminId();
        var siteA = await CreateSiteAsync(adminId, "Site A");
        var siteB = await CreateSiteAsync(adminId, "Site B");

        var first = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto("Shared Name OK", siteA.Id)) }.WithAdmin(adminId));
        var second = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto("Shared Name OK", siteB.Id)) }.WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task GetAll_FilteredBySiteId_ReturnsOnlyTerrainsForThatSite()
    {
        var adminId = NextAdminId();
        var siteA = await CreateSiteAsync(adminId, "Site A");
        var siteB = await CreateSiteAsync(adminId, "Site B");
        await CreateTerrainAsync(adminId, "Court A1", siteA.Id);
        await CreateTerrainAsync(adminId, "Court B1", siteB.Id);

        var response = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/terrains?siteId={siteB.Id}").WithAdmin(adminId));

        var terrains = await response.Content.ReadFromJsonAsync<List<TerrainDto>>();
        Assert.Single(terrains!);
        Assert.Equal("Court B1", terrains![0].Name);
    }

    [Fact]
    public async Task GetById_TerrainOnSiteOwnedByAnotherAdmin_Returns404()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId, "Site A");
        var terrain = await CreateTerrainAsync(adminId, "Court 1", site.Id);

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/terrains/{terrain.Id}").WithAdmin(NextAdminId());
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_OwnedTerrain_Returns204AndPersistsChanges()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId, "Site A");
        var terrain = await CreateTerrainAsync(adminId, "Original", site.Id);

        var updateResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"api/terrains/{terrain.Id}")
            { Content = JsonContent.Create(new UpdateTerrainDto("Updated")) }.WithAdmin(adminId));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/terrains/{terrain.Id}").WithAdmin(adminId));
        Assert.Equal("Updated", (await getResponse.Content.ReadFromJsonAsync<TerrainDto>())!.Name);
    }

    [Fact]
    public async Task Delete_OwnedTerrain_Returns204AndRemovesTerrain()
    {
        var adminId = NextAdminId();
        var site = await CreateSiteAsync(adminId, "Site A");
        var terrain = await CreateTerrainAsync(adminId, "To Delete", site.Id);

        var deleteResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"api/terrains/{terrain.Id}").WithAdmin(adminId));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/terrains/{terrain.Id}").WithAdmin(adminId));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.GetAsync("api/terrains"); 

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<SiteDto> CreateSiteAsync(int adminId, string name)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto(name, "Addr")) }.WithAdmin(adminId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    private async Task<TerrainDto> CreateTerrainAsync(int adminId, string name, int siteId)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/terrains")
            { Content = JsonContent.Create(new CreateTerrainDto(name, siteId)) }.WithAdmin(adminId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TerrainDto>())!;
    }
}
