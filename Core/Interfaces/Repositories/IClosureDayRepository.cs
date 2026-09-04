using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IClosureDayRepository
{
    Task<ClosureDay?> GetByIdAsync(int id);
    Task<IEnumerable<ClosureDay>> GetBySiteIdAsync(int siteId);
    Task<IEnumerable<ClosureDay>> GetGlobalAsync();
    Task AddAsync(ClosureDay closureDay);
    void Update(ClosureDay closureDay);
    void Delete(ClosureDay closureDay);
}
