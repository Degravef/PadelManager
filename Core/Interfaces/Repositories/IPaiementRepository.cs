using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IPaiementRepository
{
    Task<Paiement?> GetByIdAsync(int id);
    Task<IEnumerable<Paiement>> GetByMembreIdAsync(int membreId);
    Task<IEnumerable<Paiement>> GetByParticipationIdAsync(int participationId);
    Task AddAsync(Paiement paiement);
    void Update(Paiement paiement);
}
