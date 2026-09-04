using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IPenaltyRepository
{
    Task<Penalty?> GetByIdAsync(int id);
    Task<IEnumerable<Penalty>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<Penalty>> GetActiveByMemberIdAsync(int memberId);
    Task AddAsync(Penalty penalty);
    void Update(Penalty penalty);
}
