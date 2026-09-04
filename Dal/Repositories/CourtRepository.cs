using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class CourtRepository(PadelDbContext context) : ICourtRepository
{
    public async Task<Court?> GetByIdAsync(int id)
    {
        return await context.Courts.AsNoTracking().Include(t => t.Site).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Court>> GetByAdminIdAsync(int adminId, int? siteId = null)
    {
        var query = context.Courts.AsNoTracking().Where(t => t.Site!.AdminId == adminId);
        if (siteId is not null)
            query = query.Where(t => t.SiteId == siteId);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Court>> GetBySiteIdAsync(int siteId)
    {
        return await context.Courts.AsNoTracking().Where(t => t.SiteId == siteId).ToListAsync();
    }

    public async Task AddAsync(Court court)
    {
        await context.Courts.AddAsync(court);
    }

    public void Update(Court court)
    {
        context.Courts.Update(court);
    }

    public void Delete(Court court)
    {
        context.Courts.Remove(court);
    }
}
