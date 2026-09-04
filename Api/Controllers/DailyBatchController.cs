using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// RG-ETA-002/003
[ApiController]
[Route("api/admin/daily-batch")]
[Authorize(Roles = "Admin")]
public class DailyBatchController(IMatchLifecycleService matchLifecycleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DailyBatchResultDto>> Execute([FromQuery] DateOnly? date) =>
        Ok(await matchLifecycleService.ExecuteDailyBatchAsync(date));
}
