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

    // RG-PRV-001/002 : l'organisateur inscrit un joueur sur son match privé.
    [HttpPost]
    public async Task<ActionResult<ParticipationDto>> AjouterJoueur(int matchId, AjouterJoueurDto dto)
    {
        var participation = await participationService.AjouterJoueurMatchPriveAsync(User.GetMatricule(), matchId, dto);
        return CreatedAtAction(nameof(GetParticipants), new { matchId }, participation);
    }

    // RG-PUB-002/003/004 : sur un match public, chaque joueur s'inscrit lui-même.
    [HttpPost("join")]
    public async Task<ActionResult<ParticipationDto>> Rejoindre(int matchId)
    {
        var participation = await participationService.RejoindreMatchPublicAsync(User.GetMatricule(), matchId);
        return CreatedAtAction(nameof(GetParticipants), new { matchId }, participation);
    }
}
