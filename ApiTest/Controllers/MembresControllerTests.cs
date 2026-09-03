using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class MembresControllerTests(ApiTestFixture fixture)
{
    private async Task<HttpResponseMessage> CreateAsync(string type, int? siteId = null) =>
        await fixture.Client.PostAsJsonAsync("api/membres", new CreateMembreDto("Doe", "Jane", type, siteId));

    [Fact]
    public async Task Create_GlobalType_Returns201WithGeneratedMatricule()
    {
        var response = await CreateAsync("GLOBAL");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var dto = await response.Content.ReadFromJsonAsync<MembreDto>();
        Assert.StartsWith("G", dto!.Matricule);
        Assert.Equal("GLOBAL", dto.TypeMembre);
        Assert.Null(dto.SiteId);
    }

    [Fact]
    public async Task Create_TwoGlobalMembers_GetDistinctSequentialMatricules()
    {
        var first = await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MembreDto>();
        var second = await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MembreDto>();

        Assert.NotEqual(first!.Matricule, second!.Matricule);
    }

    [Fact]
    public async Task Create_SiteTypeWithoutSiteId_Returns400()
    {
        var response = await CreateAsync("SITE");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingName_Returns400()
    {
        var response = await fixture.Client.PostAsJsonAsync("api/membres", new CreateMembreDto("", "Jane", "GLOBAL", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidType_Returns400()
    {
        var response = await CreateAsync("BOGUS");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NoAuthHeaders_StillSucceeds()
    {
        
        
        var response = await fixture.Client.PostAsJsonAsync("api/membres", new CreateMembreDto("Doe", "Jane", "LIBRE", null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MembreDto>())!;

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/membres/{created.Id}")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Matricule, (await response.Content.ReadFromJsonAsync<MembreDto>())!.Matricule);
    }

    [Fact]
    public async Task GetById_AsMemberRole_Returns403()
    {
        
        
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MembreDto>())!;
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/1").WithMember(created.Matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsCreatedMembre()
    {
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MembreDto>())!;

        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var membres = await response.Content.ReadFromJsonAsync<List<MembreDto>>();
        Assert.Contains(membres!, m => m.Matricule == created.Matricule);
    }

    [Fact]
    public async Task GetAll_AsMemberRole_Returns403()
    {
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MembreDto>())!;
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres").WithMember(created.Matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ExistingMembre_ReturnsOwnDto()
    {
        var created = (await (await CreateAsync("LIBRE")).Content.ReadFromJsonAsync<MembreDto>())!;

        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/me").WithMember(created.Matricule);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Matricule, (await response.Content.ReadFromJsonAsync<MembreDto>())!.Matricule);
    }

    [Fact]
    public async Task GetMe_UnregisteredMatricule_Returns404()
    {
        
        var request = new HttpRequestMessage(HttpMethod.Get, "api/membres/me").WithMember("G99999");

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
}
