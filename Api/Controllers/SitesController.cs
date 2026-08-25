using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SitesController(ISiteService siteService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SiteDto>>> GetAll()
    {
        int adminId = User.GetAdminId();
        IEnumerable<SiteDto> sites = await siteService.GetAllSitesAsync(adminId);
        return Ok(sites);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SiteDto>> GetById(int id)
    {
        SiteDto? site = await siteService.GetSiteByIdAsync(id);
        if (site == null)
        {
            return NotFound();
        }

        return Ok(site);
    }

    [HttpPost]
    public async Task<ActionResult<SiteDto>> Create(CreateSiteDto dto)
    {
        int adminId = User.GetAdminId();
        SiteDto site = await siteService.CreateSiteAsync(adminId, dto);
        return CreatedAtAction(nameof(GetById), new { id = site.Id }, site);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSiteDto dto)
    {
        await siteService.UpdateSiteAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await siteService.DeleteSiteAsync(id);
        return NoContent();
    }
}