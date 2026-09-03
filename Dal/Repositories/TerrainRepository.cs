using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class TerrainRepository(PadelDbContext context) : ITerrainRepository
{
    public async Task<Terrain?> GetByIdAsync(int id)
    {
        return await context.Terrains.AsNoTracking().Include(t => t.Site).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Terrain>> GetByAdminIdAsync(int adminId, int? siteId = null)
    {
        var query = context.Terrains.AsNoTracking().Where(t => t.Site!.AdminId == adminId);
        if (siteId is not null)
            query = query.Where(t => t.SiteId == siteId);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Terrain>> GetBySiteIdAsync(int siteId)
    {
        return await context.Terrains.AsNoTracking().Where(t => t.SiteId == siteId).ToListAsync();
    }

    public async Task AddAsync(Terrain terrain)
    {
        await context.Terrains.AddAsync(terrain);
    }

    public void Update(Terrain terrain)
    {
        context.Terrains.Update(terrain);
    }

    public void Delete(Terrain terrain)
    {
        context.Terrains.Remove(terrain);
    }
}
