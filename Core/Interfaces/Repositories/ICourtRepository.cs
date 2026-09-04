using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ICourtRepository
{
    Task<Court?> GetByIdAsync(int id);
    Task<IEnumerable<Court>> GetByAdminIdAsync(int adminId, int? siteId = null);
    Task<IEnumerable<Court>> GetBySiteIdAsync(int siteId);
    Task AddAsync(Court court);
    void Update(Court court);
    void Delete(Court court);
}
