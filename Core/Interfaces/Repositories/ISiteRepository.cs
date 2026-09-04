using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ISiteRepository
{
    Task<Site?> GetByIdAsync(int id);
    Task<IEnumerable<Site>> GetByAdminIdAsync(int adminId);
    Task<IEnumerable<Site>> GetAllAsync();
    Task<IEnumerable<int>> GetDistinctAdminIdsAsync();
    Task AddAsync(Site site);
    void Update(Site site);
    void Delete(Site site);
}