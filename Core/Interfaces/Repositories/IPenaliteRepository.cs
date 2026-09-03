using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IPenaliteRepository
{
    Task<Penalite?> GetByIdAsync(int id);
    Task<IEnumerable<Penalite>> GetByMembreIdAsync(int membreId);
    Task<IEnumerable<Penalite>> GetActiveByMembreIdAsync(int membreId);
    Task AddAsync(Penalite penalite);
    void Update(Penalite penalite);
}
