using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/participations")]
[Authorize(Roles = "Member")]
public class ParticipationsController(IParticipationService participationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParticipationDto>>> GetParticipants(int matchId) =>
        Ok(await participationService.GetParticipantsAsync(matchId));

    // RG-PRV-001/002: the organizer registers a player onto their private match.
    [HttpPost]
    public async Task<ActionResult<ParticipationDto>> AddPlayer(int matchId, AddPlayerDto dto)
    {
        var participation = await participationService.AddPlayerToPrivateMatchAsync(User.GetMatricule(), matchId, dto);
        return CreatedAtAction(nameof(GetParticipants), new { matchId }, participation);
    }

    // RG-PUB-002/003/004: on a public match, each player registers themselves.
    [HttpPost("join")]
    public async Task<ActionResult<ParticipationDto>> Join(int matchId)
    {
        var participation = await participationService.JoinPublicMatchAsync(User.GetMatricule(), matchId);
        return CreatedAtAction(nameof(GetParticipants), new { matchId }, participation);
    }
}
