using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TerrainsController(ITerrainService terrainService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TerrainDto>>> GetAll([FromQuery] int? siteId) =>
        Ok(await terrainService.GetAllTerrainsAsync(User.GetAdminId(), siteId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TerrainDto>> GetById(int id) =>
        Ok(await terrainService.GetTerrainByIdAsync(User.GetAdminId(), id));

    [HttpPost]
    public async Task<ActionResult<TerrainDto>> Create(CreateTerrainDto dto)
    {
        var terrain = await terrainService.CreateTerrainAsync(User.GetAdminId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = terrain.Id }, terrain);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTerrainDto dto)
    {
        await terrainService.UpdateTerrainAsync(User.GetAdminId(), id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await terrainService.DeleteTerrainAsync(User.GetAdminId(), id);
        return NoContent();
    }
}
