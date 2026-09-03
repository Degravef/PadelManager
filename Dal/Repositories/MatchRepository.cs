using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class MatchRepository(PadelDbContext context) : IMatchRepository
{
    public async Task<Match?> GetByIdAsync(int id)
    {
        return await context.Matches.AsNoTracking().Include(m => m.Participations).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Match>> GetByTerrainAndDateAsync(int terrainId, DateOnly date)
    {
        return await context.Matches.AsNoTracking()
            .Where(m => m.TerrainId == terrainId && m.Date == date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetByOrganisateurIdAsync(int organisateurId)
    {
        return await context.Matches.AsNoTracking().Where(m => m.OrganisateurId == organisateurId).ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetByDateAsync(DateOnly date)
    {
        return await context.Matches.AsNoTracking()
            .Include(m => m.Participations)
            .Where(m => m.Date == date)
            .ToListAsync();
    }

    public async Task AddAsync(Match match)
    {
        await context.Matches.AddAsync(match);
    }

    public void Update(Match match)
    {
        context.Matches.Update(match);
    }
}
