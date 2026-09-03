using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IParticipationRepository
{
    Task<Participation?> GetByIdAsync(int id);
    Task<IEnumerable<Participation>> GetByMatchIdAsync(int matchId);
    Task<IEnumerable<Participation>> GetActiveByMembreIdAsync(int membreId);
    Task AddAsync(Participation participation);
    void Update(Participation participation);
    void Delete(Participation participation);
}
