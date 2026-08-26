using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IMembreRepository
{
    Task<Membre?> GetByIdAsync(int id);
    Task<Membre?> GetByMatriculeAsync(string matricule);
    Task<IEnumerable<Membre>> GetAllAsync();
    Task AddAsync(Membre membre);
}
