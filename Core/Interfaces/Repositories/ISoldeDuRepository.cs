using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ISoldeDuRepository
{
    Task<SoldeDu?> GetByIdAsync(int id);
    Task<IEnumerable<SoldeDu>> GetByMembreIdAsync(int membreId);
    Task<IEnumerable<SoldeDu>> GetOutstandingByMembreIdAsync(int membreId);
    Task<SoldeDu?> GetByMatchIdAsync(int matchId);
    Task AddAsync(SoldeDu soldeDu);
    void Update(SoldeDu soldeDu);
}
