using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembresController(IMembreService membreService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MembreDto>>> GetAll() =>
        Ok(await membreService.GetAllMembresAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MembreDto>> GetById(int id) =>
        Ok(await membreService.GetMembreByIdAsync(id));

    [HttpGet("me")]
    [Authorize(Roles = "Member")]
    public async Task<ActionResult<MembreDto>> GetMe() =>
        Ok(await membreService.GetMembreByMatriculeAsync(User.GetMatricule()));

    [HttpPost]
    [Authorize(Roles = "Member")]
    public async Task<ActionResult<MembreDto>> Create(CreateMembreDto dto)
    {
        var membre = await membreService.CreateMembreAsync(User.GetMatricule(), dto);
        return CreatedAtAction(nameof(GetById), new { id = membre.Id }, membre);
    }
}
