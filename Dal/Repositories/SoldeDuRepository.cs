using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class SoldeDuRepository(PadelDbContext context) : ISoldeDuRepository
{
    public async Task<SoldeDu?> GetByIdAsync(int id)
    {
        return await context.SoldesDus.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SoldeDu>> GetByMembreIdAsync(int membreId)
    {
        return await context.SoldesDus.AsNoTracking().Where(s => s.MembreId == membreId).ToListAsync();
    }

    public async Task<IEnumerable<SoldeDu>> GetOutstandingByMembreIdAsync(int membreId)
    {
        return await context.SoldesDus.AsNoTracking()
            .Where(s => s.MembreId == membreId && s.Statut == StatutSoldeDu.Du)
            .ToListAsync();
    }

    public async Task<SoldeDu?> GetByMatchIdAsync(int matchId)
    {
        return await context.SoldesDus.AsNoTracking().FirstOrDefaultAsync(s => s.MatchId == matchId);
    }

    public async Task AddAsync(SoldeDu soldeDu)
    {
        await context.SoldesDus.AddAsync(soldeDu);
    }

    public void Update(SoldeDu soldeDu)
    {
        context.SoldesDus.Update(soldeDu);
    }
}
