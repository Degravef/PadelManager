using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IHoraireSiteRepository
{
    Task<HoraireSite?> GetByIdAsync(int id);
    Task<HoraireSite?> GetBySiteAndYearAsync(int siteId, int annee);
    Task<IEnumerable<HoraireSite>> GetBySiteIdAsync(int siteId);
    Task AddAsync(HoraireSite horaireSite);
    void Update(HoraireSite horaireSite);
    void Delete(HoraireSite horaireSite);
}
