using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ISiteRepository
{
    Task<Site?> GetByIdAsync(int id);
    Task<IEnumerable<Site>> GetAllAsync(int adminId);
    Task AddAsync(Site site);
    void Update(Site site);
    void Delete(Site site);
    Task<bool> ExistsByNameAsync(string name);
    Task SaveChangesAsync();
}