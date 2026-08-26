using Core.Domain.Entities;
using Core.Interfaces.Repositories;

namespace Dal.Repositories;

public class MembreRepository(PadelDbContext context) : IMembreRepository
{
    public async Task<Membre?> GetByIdAsync(int id)
    {
        return await context.Membres.FindAsync(id);
    }

    public async Task AddAsync(Membre membre)
    {
        await context.Membres.AddAsync(membre);
    }
}
