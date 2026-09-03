using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class PaiementRepository(PadelDbContext context) : IPaiementRepository
{
    public async Task<Paiement?> GetByIdAsync(int id)
    {
        return await context.Paiements.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Paiement>> GetByMembreIdAsync(int membreId)
    {
        return await context.Paiements.AsNoTracking().Where(p => p.MembreId == membreId).ToListAsync();
    }

    public async Task<IEnumerable<Paiement>> GetByParticipationIdAsync(int participationId)
    {
        return await context.Paiements.AsNoTracking()
            .Where(p => p.ParticipationId == participationId)
            .ToListAsync();
    }

    public async Task AddAsync(Paiement paiement)
    {
        await context.Paiements.AddAsync(paiement);
    }

    public void Update(Paiement paiement)
    {
        context.Paiements.Update(paiement);
    }
}
