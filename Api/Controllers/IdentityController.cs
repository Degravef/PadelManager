using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IdentityController(ISiteService siteService, IMemberService memberService) : ControllerBase
{
    [HttpGet("admin-ids")]
    public async Task<ActionResult<IEnumerable<int>>> GetAdminIds() =>
        Ok(await siteService.GetAllAdminIdsAsync());

    [HttpGet("matricules")]
    public async Task<ActionResult<IEnumerable<string>>> GetMatricules() =>
        Ok(await memberService.GetAllMatriculesAsync());
}
