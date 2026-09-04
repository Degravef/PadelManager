using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByMatriculeAsync(string matricule);
    Task<IEnumerable<Member>> GetAllAsync();
    Task<IEnumerable<string>> GetAllMatriculesAsync();
    Task<IEnumerable<string>> GetMatriculesByPrefixAsync(string prefix);
    Task AddAsync(Member member);
}
