using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Backs the member-facing reservation form: SitesController/CourtsController are
// ownership-scoped to the calling admin (CF-AA-005), but a Member booking a court must
// be able to browse every site/court, not just one admin's — see DOMAIN_RULES.md §2.
[ApiController]
[Route("api/reservations/lookup")]
[Authorize(Roles = "Member")]
public class ReservationLookupController(
    ISiteService siteService,
    ICourtService courtService,
    IReservationService reservationService) : ControllerBase
{
    [HttpGet("sites")]
    public async Task<ActionResult<IEnumerable<SiteDto>>> GetSites() =>
        Ok(await siteService.GetAllSitesPublicAsync());

    [HttpGet("sites/{siteId:int}/courts")]
    public async Task<ActionResult<IEnumerable<CourtDto>>> GetCourts(int siteId) =>
        Ok(await courtService.GetCourtsBySiteAsync(siteId));

    [HttpGet("sites/{siteId:int}/slots")]
    public async Task<ActionResult<IEnumerable<AvailableSlotDto>>> GetAvailableSlots(int siteId, [FromQuery] DateOnly date) =>
        Ok(await reservationService.GetAvailableSlotsAsync(siteId, date));
}
