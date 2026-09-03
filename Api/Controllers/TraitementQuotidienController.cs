using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// RG-ETA-002/003 : traitement quotidien J-1 (bascules privé->public, pénalités, soldes dus). Il n'y a
// pas de BackgroundService (voir BACKLOG.md) — un déclenchement manuel par un admin est le substitut
// accepté. Opération système, non limitée aux sites d'un admin en particulier (voir DOMAIN_RULES.md §5) :
// n'importe quel admin authentifié peut la déclencher, elle traite tous les sites de tous les admins.
[ApiController]
[Route("api/admin/traitement-quotidien")]
[Authorize(Roles = "Admin")]
public class TraitementQuotidienController(IMatchLifecycleService matchLifecycleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TraitementQuotidienResultDto>> Executer([FromQuery] DateOnly? date) =>
        Ok(await matchLifecycleService.ExecuterTraitementQuotidienAsync(date));
}
