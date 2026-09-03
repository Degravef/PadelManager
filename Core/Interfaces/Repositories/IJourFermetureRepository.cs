using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IJourFermetureRepository
{
    Task<JourFermeture?> GetByIdAsync(int id);
    Task<IEnumerable<JourFermeture>> GetBySiteIdAsync(int siteId);
    Task<IEnumerable<JourFermeture>> GetGlobalAsync();
    Task AddAsync(JourFermeture jourFermeture);
    void Update(JourFermeture jourFermeture);
    void Delete(JourFermeture jourFermeture);
}
