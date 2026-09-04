using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// RG-ETA-002/003: the J-1 daily batch (private->public switches, penalties, balances due). There is
// no BackgroundService (see BACKLOG.md) — a manual trigger by an admin is the accepted substitute.
// System-wide operation, not scoped to any one admin's sites (see DOMAIN_RULES.md §5): any
// authenticated admin can trigger it, and it processes every site of every admin.
[ApiController]
[Route("api/admin/daily-batch")]
[Authorize(Roles = "Admin")]
public class DailyBatchController(IMatchLifecycleService matchLifecycleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DailyBatchResultDto>> Execute([FromQuery] DateOnly? date) =>
        Ok(await matchLifecycleService.ExecuteDailyBatchAsync(date));
}
