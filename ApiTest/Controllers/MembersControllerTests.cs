using System.Net;
using System.Net.Http.Json;
using ApiTest.Helpers;
using Core.Dtos;

namespace ApiTest.Controllers;

[Collection("Api")]
public class MembersControllerTests(ApiTestFixture fixture)
{
    private async Task<HttpResponseMessage> CreateAsync(string type, int? siteId = null) =>
        await fixture.Client.PostAsJsonAsync("api/members", new CreateMemberDto("Doe", "Jane", type, siteId));

    [Fact]
    public async Task Create_GlobalType_Returns201WithGeneratedMatricule()
    {
        var response = await CreateAsync("GLOBAL");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var dto = await response.Content.ReadFromJsonAsync<MemberDto>();
        Assert.StartsWith("G", dto!.Matricule);
        Assert.Equal("GLOBAL", dto.MemberType);
        Assert.Null(dto.SiteId);
    }

    [Fact]
    public async Task Create_TwoGlobalMembers_GetDistinctSequentialMatricules()
    {
        var first = await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MemberDto>();
        var second = await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MemberDto>();

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
        var response = await fixture.Client.PostAsJsonAsync("api/members", new CreateMemberDto("", "Jane", "GLOBAL", null));

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
        // Registration must be reachable before the caller has any matricule to send as X-User-Id —
        // see the ASSUMPTION comment on MembersController.Create.
        var response = await fixture.Client.PostAsJsonAsync("api/members", new CreateMemberDto("Doe", "Jane", "LIBRE", null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/members/999999")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingMember_ReturnsDto()
    {
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MemberDto>())!;

        var response = await fixture.Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/members/{created.Id}")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Matricule, (await response.Content.ReadFromJsonAsync<MemberDto>())!.Matricule);
    }

    [Fact]
    public async Task GetById_AsMemberRole_Returns403()
    {
        // GetById is reserved for Admin (EF-ADM-003: member list/stats is an admin capability) — a
        // Member looks up their own record via GET api/members/me instead, never by another id.
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MemberDto>())!;
        var request = new HttpRequestMessage(HttpMethod.Get, "api/members/1").WithMember(created.Matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsCreatedMember()
    {
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MemberDto>())!;

        var request = new HttpRequestMessage(HttpMethod.Get, "api/members")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<MemberDto>>();
        Assert.Contains(members!, m => m.Matricule == created.Matricule);
    }

    [Fact]
    public async Task GetAll_AsMemberRole_Returns403()
    {
        var created = (await (await CreateAsync("GLOBAL")).Content.ReadFromJsonAsync<MemberDto>())!;
        var request = new HttpRequestMessage(HttpMethod.Get, "api/members").WithMember(created.Matricule);

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ExistingMember_ReturnsOwnDto()
    {
        var created = (await (await CreateAsync("LIBRE")).Content.ReadFromJsonAsync<MemberDto>())!;

        var request = new HttpRequestMessage(HttpMethod.Get, "api/members/me").WithMember(created.Matricule);
        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Matricule, (await response.Content.ReadFromJsonAsync<MemberDto>())!.Matricule);
    }

    [Fact]
    public async Task GetMe_UnregisteredMatricule_Returns404()
    {
        // Valid format (5-digit cap) but never issued — matricules are generated sequentially from 1.
        var request = new HttpRequestMessage(HttpMethod.Get, "api/members/me").WithMember("G99999");

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_AsAdminRole_Returns403()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/members/me")
            .WithAdmin(Random.Shared.Next(100_000, int.MaxValue));

        var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
