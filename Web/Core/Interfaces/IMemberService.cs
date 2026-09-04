using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IMemberService
{
    Task<MemberDto> GetMemberByIdAsync(int id);
    Task<MemberDto> GetMyMemberAsync();
    Task<IEnumerable<MemberDto>> GetAllMembersAsync();
    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto);
}
