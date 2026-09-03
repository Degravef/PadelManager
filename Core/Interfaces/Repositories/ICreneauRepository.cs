using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ICreneauRepository
{
    Task<Creneau?> GetByIdAsync(int id);
    Task<IEnumerable<Creneau>> GetByHoraireSiteIdAsync(int horaireSiteId);
    Task AddRangeAsync(IEnumerable<Creneau> creneaux);
}
