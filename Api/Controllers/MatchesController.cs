using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Member")]
public class MatchesController(IReservationService reservationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatchDto>> GetById(int id) =>
        Ok(await reservationService.GetReservationByIdAsync(User.GetMatricule(), id));

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMine() =>
        Ok(await reservationService.GetMyReservationsAsync(User.GetMatricule()));

    [HttpPost]
    public async Task<ActionResult<MatchDto>> Create(CreerReservationDto dto)
    {
        var match = await reservationService.CreerReservationAsync(User.GetMatricule(), dto);
        return CreatedAtAction(nameof(GetById), new { id = match.Id }, match);
    }
}
