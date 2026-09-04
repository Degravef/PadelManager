using Api.Extensions;
using Core.Dtos;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetAll() =>
        Ok(await memberService.GetAllMembersAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MemberDto>> GetById(int id) =>
        Ok(await memberService.GetMemberByIdAsync(id));

    [HttpGet("me")]
    [Authorize(Roles = "Member")]
    public async Task<ActionResult<MemberDto>> GetMe() =>
        Ok(await memberService.GetMemberByMatriculeAsync(User.GetMatricule()));

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberDto dto)
    {
        var member = await memberService.CreateMemberAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }
}
