using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "Member")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    // Lists the caller's unpaid balances — needed to know which id to pass to PayBalanceDue.
    [HttpGet("balances/me")]
    public async Task<ActionResult<IEnumerable<BalanceDueDto>>> GetMyBalances() =>
        Ok(await paymentService.GetMyUnpaidBalancesAsync(User.GetMatricule()));

    // RG-PAY-003/007/008: pays a reserved seat, absorbing any outstanding balance due along the way.
    [HttpPost("participations/{participationId:int}/payments")]
    public async Task<ActionResult<PaymentDto>> PayParticipation(int participationId, PayDto dto) =>
        Ok(await paymentService.PayParticipationAsync(User.GetMatricule(), participationId, dto));

    // RG-PAY-005/006: settles a balance due directly, without waiting to join another match.
    [HttpPost("balances/{balanceDueId:int}/payments")]
    public async Task<ActionResult<PaymentDto>> PayBalanceDue(int balanceDueId, PayDto dto) =>
        Ok(await paymentService.PayBalanceDueAsync(User.GetMatricule(), balanceDueId, dto));
}
