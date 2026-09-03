using Core.Domain.Entities;
using Core.Domain.Enums;
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

    public async Task<IEnumerable<Paiement>> GetValidatedBySitesAndPeriodAsync(IEnumerable<int> siteIds, DateOnly debut, DateOnly fin)
    {
        List<int> ids = siteIds.ToList();

        IQueryable<Paiement> viaParticipation = context.Paiements.AsNoTracking()
            .Where(p => p.Statut == StatutPaiement.Valide && p.ParticipationId != null)
            .Where(p => ids.Contains(p.Participation!.Match!.Terrain!.SiteId) &&
                        p.Participation!.Match!.Date >= debut && p.Participation!.Match!.Date <= fin);

        IQueryable<Paiement> viaSolde = context.Paiements.AsNoTracking()
            .Where(p => p.Statut == StatutPaiement.Valide && p.SoldeDuId != null)
            .Where(p => ids.Contains(p.SoldeDu!.Match!.Terrain!.SiteId) &&
                        p.SoldeDu!.Match!.Date >= debut && p.SoldeDu!.Match!.Date <= fin);

        return await viaParticipation.Union(viaSolde).ToListAsync();
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
