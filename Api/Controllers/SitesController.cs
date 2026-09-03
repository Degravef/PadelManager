using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]   
public class SitesController(ISiteService siteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SiteDto>>> GetAll() =>
        Ok(await siteService.GetAllSitesAsync(User.GetAdminId()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SiteDto>> GetById(int id) =>
        Ok(await siteService.GetSiteByIdAsync(User.GetAdminId(), id));

    [HttpPost]
    public async Task<ActionResult<SiteDto>> Create(CreateSiteDto dto)
    {
        var site = await siteService.CreateSiteAsync(User.GetAdminId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = site.Id }, site);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSiteDto dto)
    {
        await siteService.UpdateSiteAsync(User.GetAdminId(), id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await siteService.DeleteSiteAsync(User.GetAdminId(), id);
        return NoContent();
    }
}