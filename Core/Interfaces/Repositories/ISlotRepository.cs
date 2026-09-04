using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ISlotRepository
{
    Task<Slot?> GetByIdAsync(int id);
    Task<IEnumerable<Slot>> GetBySiteScheduleIdAsync(int siteScheduleId);
    Task AddRangeAsync(IEnumerable<Slot> slots);
}
