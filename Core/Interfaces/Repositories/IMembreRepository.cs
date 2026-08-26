using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IMembreRepository
{
    Task<Membre?> GetByIdAsync(int id);
    Task AddAsync(Membre membre);
}
