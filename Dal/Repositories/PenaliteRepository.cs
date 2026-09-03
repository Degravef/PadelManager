using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class PenaliteRepository(PadelDbContext context) : IPenaliteRepository
{
    public async Task<Penalite?> GetByIdAsync(int id)
    {
        return await context.Penalites.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Penalite>> GetByMembreIdAsync(int membreId)
    {
        return await context.Penalites.AsNoTracking().Where(p => p.MembreId == membreId).ToListAsync();
    }

    public async Task<IEnumerable<Penalite>> GetActiveByMembreIdAsync(int membreId)
    {
        return await context.Penalites.AsNoTracking()
            .Where(p => p.MembreId == membreId && p.Active)
            .ToListAsync();
    }

    public async Task AddAsync(Penalite penalite)
    {
        await context.Penalites.AddAsync(penalite);
    }

    public void Update(Penalite penalite)
    {
        context.Penalites.Update(penalite);
    }
}
