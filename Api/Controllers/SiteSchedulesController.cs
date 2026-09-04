using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/sites/{siteId:int}/schedules")]
[Authorize(Roles = "Admin")]
public class SiteSchedulesController(ISiteScheduleService siteScheduleService) : ControllerBase
{
    [HttpGet("{year:int}")]
    public async Task<ActionResult<SiteScheduleDto>> GetByYear(int siteId, int year) =>
        Ok(await siteScheduleService.GetBySiteAndYearAsync(User.GetAdminId(), siteId, year));

    [HttpPost]
    public async Task<ActionResult<SiteScheduleDto>> Create(int siteId, CreateSiteScheduleDto dto)
    {
        var schedule = await siteScheduleService.CreateAsync(User.GetAdminId(), siteId, dto);
        return CreatedAtAction(nameof(GetByYear), new { siteId, year = schedule.Year }, schedule);
    }
}
