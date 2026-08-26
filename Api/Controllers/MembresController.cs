using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Member")]
public class MembresController(IMembreService membreService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembreDto>> GetById(int id) =>
        Ok(await membreService.GetMembreByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<MembreDto>> Create(CreateMembreDto dto)
    {
        var membre = await membreService.CreateMembreAsync(User.GetMatricule(), dto);
        return CreatedAtAction(nameof(GetById), new { id = membre.Id }, membre);
    }
}
