using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IPaiementRepository
{
    Task<Paiement?> GetByIdAsync(int id);
    Task<IEnumerable<Paiement>> GetByMembreIdAsync(int membreId);
    Task<IEnumerable<Paiement>> GetByParticipationIdAsync(int participationId);

    // RG-PAY-009 / CF-RC-004: validated payments for matches on the given sites within [debut, fin],
    // whether attached to a Participation (regular seat payment) or a SoldeDu (backfilled balance).
    Task<IEnumerable<Paiement>> GetValidatedBySitesAndPeriodAsync(IEnumerable<int> siteIds, DateOnly debut, DateOnly fin);
    Task AddAsync(Paiement paiement);
    void Update(Paiement paiement);
}
