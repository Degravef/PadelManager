using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IMemberService
{
    Task<MemberDto> GetMemberByIdAsync(int id);
    Task<MemberDto> GetMemberByMatriculeAsync(string matricule);
    Task<IEnumerable<MemberDto>> GetAllMembersAsync();
    Task<IEnumerable<string>> GetAllMatriculesAsync();
    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto);
}
