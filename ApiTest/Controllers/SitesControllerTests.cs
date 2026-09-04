using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class SitesControllerTests(ApiTestFixture fixture)
{
    private static int NextAdminId() => Random.Shared.Next(100_000, int.MaxValue);

    [Fact]
    public async Task GetAll_NoSitesForAdmin_ReturnsEmptyList()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/sites").WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<SiteDto>>())!);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201WithLocationHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto("Site A", "Address A")) }.WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("Site A", (await response.Content.ReadFromJsonAsync<SiteDto>())!.Name);
    }

    [Fact]
    public async Task Create_MissingName_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto("", "Address A")) }.WithAdmin(NextAdminId());

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateNameForSameAdmin_Returns409()
    {
        var adminId = NextAdminId();
        var dto = new CreateSiteDto("Duplicate Site", "Address A");

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(dto) }.WithAdmin(adminId))).EnsureSuccessStatusCode();

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(dto) }.WithAdmin(adminId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_SameNameDifferentAdmins_BothSucceed()
    {
        var dto = new CreateSiteDto("Shared Name OK", "Address A");

        var first = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(dto) }.WithAdmin(NextAdminId()));
        var second = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(dto) }.WithAdmin(NextAdminId()));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task GetById_SiteOwnedByAnotherAdmin_Returns404()
    {
        var created = await CreateSiteAsync(NextAdminId(), "Private Site", "Addr");

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/sites/{created.Id}").WithAdmin(NextAdminId());
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_OwnedSite_Returns204AndPersistsChanges()
    {
        var adminId = NextAdminId();
        var created = await CreateSiteAsync(adminId, "Original", "Addr");

        var updateResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"api/sites/{created.Id}")
            { Content = JsonContent.Create(new UpdateSiteDto("Updated", "New Addr")) }.WithAdmin(adminId));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/sites/{created.Id}").WithAdmin(adminId));
        Assert.Equal("Updated", (await getResponse.Content.ReadFromJsonAsync<SiteDto>())!.Name);
    }

    [Fact]
    public async Task Delete_OwnedSite_Returns204AndRemovesSite()
    {
        var adminId = NextAdminId();
        var created = await CreateSiteAsync(adminId, "To Delete", "Addr");

        var deleteResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"api/sites/{created.Id}").WithAdmin(adminId));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await fixture.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"api/sites/{created.Id}").WithAdmin(adminId));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.GetAsync("api/sites"); // no .WithAdmin(...)

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_NonNumericAdminId_Returns400()
    {
        // HeaderHandler lets authentication pass (the headers are present); it's
        // ContextExtensions.GetAdminId() that rejects a non-numeric X-User-Id via InvalidAdminIdException.
        var request = new HttpRequestMessage(HttpMethod.Get, "api/sites");
        request.Headers.Add("X-User-Role", "Admin");
        request.Headers.Add("X-User-Id", "not-a-number");

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<SiteDto> CreateSiteAsync(int adminId, string name, string address)
    {
        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/sites")
            { Content = JsonContent.Create(new CreateSiteDto(name, address)) }.WithAdmin(adminId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }
}