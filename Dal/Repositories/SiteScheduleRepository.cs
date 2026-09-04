using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class SiteScheduleRepository(PadelDbContext context) : ISiteScheduleRepository
{
    public async Task<SiteSchedule?> GetByIdAsync(int id)
    {
        return await context.SiteSchedules.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<SiteSchedule?> GetBySiteAndYearAsync(int siteId, int year)
    {
        return await context.SiteSchedules.AsNoTracking()
            .FirstOrDefaultAsync(h => h.SiteId == siteId && h.Year == year);
    }

    public async Task<IEnumerable<SiteSchedule>> GetBySiteIdAsync(int siteId)
    {
        return await context.SiteSchedules.AsNoTracking().Where(h => h.SiteId == siteId).ToListAsync();
    }

    public async Task AddAsync(SiteSchedule siteSchedule)
    {
        await context.SiteSchedules.AddAsync(siteSchedule);
    }

    public void Update(SiteSchedule siteSchedule)
    {
        context.SiteSchedules.Update(siteSchedule);
    }

    public void Delete(SiteSchedule siteSchedule)
    {
        context.SiteSchedules.Remove(siteSchedule);
    }
}
