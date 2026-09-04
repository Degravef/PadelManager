using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IParticipationRepository
{
    Task<Participation?> GetByIdAsync(int id);
    Task<IEnumerable<Participation>> GetByMatchIdAsync(int matchId);
    Task<IEnumerable<Participation>> GetActiveByMemberIdAsync(int memberId);
    Task AddAsync(Participation participation);
    void Update(Participation participation);
    void Delete(Participation participation);
}
