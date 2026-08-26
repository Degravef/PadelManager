using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Backs the member-facing reservation form: SitesController/TerrainsController are
// ownership-scoped to the calling admin (CF-AA-005), but a Member booking a court must
// be able to browse every site/terrain, not just one admin's — see DOMAIN_RULES.md §2.
[ApiController]
[Route("api/reservations/lookup")]
[Authorize(Roles = "Member")]
public class ReservationLookupController(ISiteService siteService, ITerrainService terrainService) : ControllerBase
{
    [HttpGet("sites")]
    public async Task<ActionResult<IEnumerable<SiteDto>>> GetSites() =>
        Ok(await siteService.GetAllSitesPublicAsync());

    [HttpGet("sites/{siteId:int}/terrains")]
    public async Task<ActionResult<IEnumerable<TerrainDto>>> GetTerrains(int siteId) =>
        Ok(await terrainService.GetTerrainsBySiteAsync(siteId));
}
