using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class TypeMembreRepository(PadelDbContext context) : ITypeMembreRepository
{
    public async Task<TypeMembre?> GetByCodeAsync(string code)
    {
        return await context.TypeMembres.FirstOrDefaultAsync(t => t.Code == code);
    }
}
