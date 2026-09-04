using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class SlotRepository(PadelDbContext context) : ISlotRepository
{
    public async Task<Slot?> GetByIdAsync(int id)
    {
        return await context.Slots.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Slot>> GetBySiteScheduleIdAsync(int siteScheduleId)
    {
        return await context.Slots.AsNoTracking()
            .Where(c => c.SiteScheduleId == siteScheduleId)
            .OrderBy(c => c.Order)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Slot> slots)
    {
        await context.Slots.AddRangeAsync(slots);
    }
}
