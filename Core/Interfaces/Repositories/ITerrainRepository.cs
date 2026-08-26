using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ITerrainRepository
{
    Task<Terrain?> GetByIdAsync(int id);
    Task<IEnumerable<Terrain>> GetByAdminIdAsync(int adminId, int? siteId = null);
    Task AddAsync(Terrain terrain);
    void Update(Terrain terrain);
    void Delete(Terrain terrain);
}
