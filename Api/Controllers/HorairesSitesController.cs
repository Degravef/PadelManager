using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/sites/{siteId:int}/horaires")]
[Authorize(Roles = "Admin")]
public class HorairesSitesController(IHoraireSiteService horaireSiteService) : ControllerBase
{
    [HttpGet("{annee:int}")]
    public async Task<ActionResult<HoraireSiteDto>> GetByYear(int siteId, int annee) =>
        Ok(await horaireSiteService.GetBySiteAndYearAsync(User.GetAdminId(), siteId, annee));

    [HttpPost]
    public async Task<ActionResult<HoraireSiteDto>> Create(int siteId, CreateHoraireSiteDto dto)
    {
        var horaire = await horaireSiteService.CreateAsync(User.GetAdminId(), siteId, dto);
        return CreatedAtAction(nameof(GetByYear), new { siteId, annee = horaire.Annee }, horaire);
    }
}
