using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CourtsController(ICourtService courtService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourtDto>>> GetAll([FromQuery] int? siteId) =>
        Ok(await courtService.GetAllCourtsAsync(User.GetAdminId(), siteId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourtDto>> GetById(int id) =>
        Ok(await courtService.GetCourtByIdAsync(User.GetAdminId(), id));

    [HttpPost]
    public async Task<ActionResult<CourtDto>> Create(CreateCourtDto dto)
    {
        var court = await courtService.CreateCourtAsync(User.GetAdminId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = court.Id }, court);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCourtDto dto)
    {
        await courtService.UpdateCourtAsync(User.GetAdminId(), id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await courtService.DeleteCourtAsync(User.GetAdminId(), id);
        return NoContent();
    }
}
