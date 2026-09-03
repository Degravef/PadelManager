using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IMembreRepository
{
    Task<Membre?> GetByIdAsync(int id);
    Task<Membre?> GetByMatriculeAsync(string matricule);
    Task<IEnumerable<Membre>> GetAllAsync();
    Task<IEnumerable<string>> GetAllMatriculesAsync();
    Task<IEnumerable<string>> GetMatriculesByPrefixAsync(string prefix);
    Task AddAsync(Membre membre);
}
