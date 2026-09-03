using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;





[ApiController]
[Route("api/admin/traitement-quotidien")]
[Authorize(Roles = "Admin")]
public class TraitementQuotidienController(IMatchLifecycleService matchLifecycleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TraitementQuotidienResultDto>> Executer([FromQuery] DateOnly? date) =>
        Ok(await matchLifecycleService.ExecuterTraitementQuotidienAsync(date));
}
