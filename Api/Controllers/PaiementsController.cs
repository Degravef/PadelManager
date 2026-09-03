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
    // Liste les soldes impayés de l'appelant — nécessaire pour connaître l'ID à passer à PayerSolde.
    [HttpGet("soldes/me")]
    public async Task<ActionResult<IEnumerable<SoldeDuDto>>> GetMesSoldes() =>
        Ok(await paiementService.GetMesSoldesImpayesAsync(User.GetMatricule()));

    // RG-PAY-003/007/008 : paie une place réservée, absorbe un solde impayé éventuel.
    [HttpPost("participations/{participationId:int}/paiements")]
    public async Task<ActionResult<PaiementDto>> PayerParticipation(int participationId, PayerDto dto) =>
        Ok(await paiementService.PayerParticipationAsync(User.GetMatricule(), participationId, dto));

    // RG-PAY-005/006 : solde un solde dû directement, sans attendre de rejoindre un autre match.
    [HttpPost("soldes/{soldeDuId:int}/paiements")]
    public async Task<ActionResult<PaiementDto>> PayerSolde(int soldeDuId, PayerDto dto) =>
        Ok(await paiementService.PayerSoldeAsync(User.GetMatricule(), soldeDuId, dto));
}
