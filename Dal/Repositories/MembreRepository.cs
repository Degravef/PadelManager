using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class MembreRepository(PadelDbContext context) : IMembreRepository
{
    public async Task<Membre?> GetByIdAsync(int id)
    {
        return await context.Membres.FindAsync(id);
    }

    public async Task<Membre?> GetByMatriculeAsync(string matricule)
    {
        return await context.Membres.FirstOrDefaultAsync(m => m.Matricule == matricule);
    }

    public async Task<IEnumerable<Membre>> GetAllAsync()
    {
        return await context.Membres.ToListAsync();
    }

    public async Task AddAsync(Membre membre)
    {
        await context.Membres.AddAsync(membre);
    }
}
