using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "Member")]
public class PaiementsController(IPaiementService paiementService) : ControllerBase
{
    
    [HttpGet("soldes/me")]
    public async Task<ActionResult<IEnumerable<SoldeDuDto>>> GetMesSoldes() =>
        Ok(await paiementService.GetMesSoldesImpayesAsync(User.GetMatricule()));

    
    [HttpPost("participations/{participationId:int}/paiements")]
    public async Task<ActionResult<PaiementDto>> PayerParticipation(int participationId, PayerDto dto) =>
        Ok(await paiementService.PayerParticipationAsync(User.GetMatricule(), participationId, dto));

    
    [HttpPost("soldes/{soldeDuId:int}/paiements")]
    public async Task<ActionResult<PaiementDto>> PayerSolde(int soldeDuId, PayerDto dto) =>
        Ok(await paiementService.PayerSoldeAsync(User.GetMatricule(), soldeDuId, dto));
}
