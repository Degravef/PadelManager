using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class MembresControllerTests(ApiTestFixture fixture)
{
    private static string NextMatricule(char prefix) => $"{prefix}{Random.Shared.Next(10_000, 99_999)}";

    [Fact]
    public async Task Create_GlobalMatricule_Returns201WithDerivedType()
    {
        var matricule = NextMatricule('G');
        var request = new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var dto = await response.Content.ReadFromJsonAsync<MembreDto>();
        Assert.Equal(matricule, dto!.Matricule);
        Assert.Equal("GLOBAL", dto.TypeMembre);
        Assert.Null(dto.SiteId);
    }

    [Fact]
    public async Task Create_SiteMatriculeWithoutSiteId_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(NextMatricule('S'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingName_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("", "Jane", null)) }.WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidMatriculeFormat_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember("X123");

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateMatricule_Returns409()
    {
        var matricule = NextMatricule('L');
        var dto = new CreateMembreDto("Doe", "Jane", null);

        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(dto) }.WithMember(matricule))).EnsureSuccessStatusCode();

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(dto) }.WithMember(matricule));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/999999")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingMembre_ReturnsDto()
    {
        var matricule = NextMatricule('G');
        var createResponse = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(matricule));
        var created = (await createResponse.Content.ReadFromJsonAsync<MembreDto>())!;

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/membres/{created.Id}")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(matricule, (await response.Content.ReadFromJsonAsync<MembreDto>())!.Matricule);
    }

    [Fact]
    public async Task GetById_AsMemberRole_Returns403()
    {
        // GetById is reserved for Admin (EF-ADM-003 : member list/stats is an admin capability) — a
        // Member looks up their own record via GET api/membres/me instead, never by another id.
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/1").WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsCreatedMembre()
    {
        var matricule = NextMatricule('G');
        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(matricule)))
            .EnsureSuccessStatusCode();

        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var membres = await response.Content.ReadFromJsonAsync<List<MembreDto>>();
        Assert.Contains(membres!, m => m.Matricule == matricule);
    }

    [Fact]
    public async Task GetAll_AsMemberRole_Returns403()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres").WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ExistingMembre_ReturnsOwnDto()
    {
        var matricule = NextMatricule('L');
        (await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }.WithMember(matricule)))
            .EnsureSuccessStatusCode();

        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/me").WithMember(matricule);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(matricule, (await response.Content.ReadFromJsonAsync<MembreDto>())!.Matricule);
    }

    [Fact]
    public async Task GetMe_UnregisteredMatricule_Returns404()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/me").WithMember(NextMatricule('G'));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_AsAdminRole_Returns403()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/me")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingAuthHeaders_Returns401()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/membres", new CreateMembreDto("Doe", "Jane", null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AdminRoleInstead_Returns403()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/membres")
            { Content = JsonContent.Create(new CreateMembreDto("Doe", "Jane", null)) }
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
