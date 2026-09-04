using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// RG-PAY-009 / CF-RC-004. "Per site" vs "global" mirrors EF-ADM-002 (DOMAIN_RULES.md §5/§7): global
// means consolidated across every site owned by the calling admin, never across other admins' sites.
[ApiController]
[Route("api/statistics")]
[Authorize(Roles = "Admin")]
public class StatisticsController(IStatisticsService statisticsService, ISiteService siteService) : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueDto>> GetRevenue(
        [FromQuery] int? siteId, [FromQuery] DateOnly start, [FromQuery] DateOnly end)
    {
        int adminId = User.GetAdminId();

        IEnumerable<int> siteIds;
        if (siteId is not null)
        {
            await siteService.GetSiteByIdAsync(adminId, siteId.Value); // throws SiteNotFoundException if not owned
            siteIds = [siteId.Value];
        }
        else
        {
            siteIds = (await siteService.GetAllSitesAsync(adminId)).Select(s => s.Id);
        }

        return Ok(await statisticsService.CalculateRevenueAsync(siteIds, start, end));
    }
}
