using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Not tied to a business aggregate: backs the front-end's identity switcher, which needs
// to list the ids someone can act as before any X-User-Role/X-User-Id header is sent.
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IdentityController(ISiteService siteService, IMembreService membreService) : ControllerBase
{
    [HttpGet("admin-ids")]
    public async Task<ActionResult<IEnumerable<int>>> GetAdminIds() =>
        Ok(await siteService.GetAllAdminIdsAsync());

    [HttpGet("matricules")]
    public async Task<ActionResult<IEnumerable<string>>> GetMatricules() =>
        Ok(await membreService.GetAllMatriculesAsync());
}
