using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;




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
