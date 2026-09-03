using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ITypeMembreRepository
{
    Task<TypeMembre?> GetByCodeAsync(string code);
}
