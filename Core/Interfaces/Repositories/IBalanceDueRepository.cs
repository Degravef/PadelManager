using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IBalanceDueRepository
{
    Task<BalanceDue?> GetByIdAsync(int id);
    Task<IEnumerable<BalanceDue>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<BalanceDue>> GetOutstandingByMemberIdAsync(int memberId);
    Task<BalanceDue?> GetByMatchIdAsync(int matchId);
    Task AddAsync(BalanceDue balanceDue);
    void Update(BalanceDue balanceDue);
}
