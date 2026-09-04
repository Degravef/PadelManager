using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class ClosureDayRepository(PadelDbContext context) : IClosureDayRepository
{
    public async Task<ClosureDay?> GetByIdAsync(int id)
    {
        return await context.ClosureDays.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<ClosureDay>> GetBySiteIdAsync(int siteId)
    {
        return await context.ClosureDays.AsNoTracking().Where(j => j.SiteId == siteId).ToListAsync();
    }

    public async Task<IEnumerable<ClosureDay>> GetGlobalAsync()
    {
        return await context.ClosureDays.AsNoTracking().Where(j => j.SiteId == null).ToListAsync();
    }

    public async Task AddAsync(ClosureDay closureDay)
    {
        await context.ClosureDays.AddAsync(closureDay);
    }

    public void Update(ClosureDay closureDay)
    {
        context.ClosureDays.Update(closureDay);
    }

    public void Delete(ClosureDay closureDay)
    {
        context.ClosureDays.Remove(closureDay);
    }
}
