using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;



[ApiController]
[Route("api/statistiques")]
[Authorize(Roles = "Admin")]
public class StatistiquesController(IStatistiquesService statistiquesService, ISiteService siteService) : ControllerBase
{
    [HttpGet("chiffre-affaires")]
    public async Task<ActionResult<ChiffreAffairesDto>> GetChiffreAffaires(
        [FromQuery] int? siteId, [FromQuery] DateOnly debut, [FromQuery] DateOnly fin)
    {
        int adminId = User.GetAdminId();

        IEnumerable<int> siteIds;
        if (siteId is not null)
        {
            await siteService.GetSiteByIdAsync(adminId, siteId.Value); 
            siteIds = [siteId.Value];
        }
        else
        {
            siteIds = (await siteService.GetAllSitesAsync(adminId)).Select(s => s.Id);
        }

        return Ok(await statistiquesService.CalculerChiffreAffairesAsync(siteIds, debut, fin));
    }
}
