using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class MembreRepository(PadelDbContext context) : IMembreRepository
{
    public async Task<Membre?> GetByIdAsync(int id)
    {
        return await context.Membres.Include(m => m.TypeMembre).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Membre?> GetByMatriculeAsync(string matricule)
    {
        return await context.Membres.Include(m => m.TypeMembre).FirstOrDefaultAsync(m => m.Matricule == matricule);
    }

    public async Task<IEnumerable<Membre>> GetAllAsync()
    {
        return await context.Membres.Include(m => m.TypeMembre).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllMatriculesAsync()
    {
        return await context.Membres.Select(m => m.Matricule).ToListAsync();
    }

    public async Task AddAsync(Membre membre)
    {
        await context.Membres.AddAsync(membre);
    }
}
