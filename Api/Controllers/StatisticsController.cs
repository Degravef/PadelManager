using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// RG-PAY-009 / CF-RC-004
/// EF-ADM-002
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
            await siteService.GetSiteByIdAsync(adminId, siteId.Value);
            siteIds = [siteId.Value];
        }
        else
        {
            siteIds = (await siteService.GetAllSitesAsync(adminId)).Select(s => s.Id);
        }

        return Ok(await statisticsService.CalculateRevenueAsync(siteIds, start, end));
    }
}
