using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class MembreRepository(PadelDbContext context) : IMembreRepository
{
    public async Task<Membre?> GetByIdAsync(int id)
    {
        return await context.Membres.AsNoTracking()
            .Include(m => m.TypeMembre).Include(m => m.SoldesDus).Include(m => m.Penalites)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Membre?> GetByMatriculeAsync(string matricule)
    {
        return await context.Membres.AsNoTracking()
            .Include(m => m.TypeMembre).Include(m => m.SoldesDus).Include(m => m.Penalites)
            .FirstOrDefaultAsync(m => m.Matricule == matricule);
    }

    public async Task<IEnumerable<Membre>> GetAllAsync()
    {
        return await context.Membres.AsNoTracking()
            .Include(m => m.TypeMembre).Include(m => m.SoldesDus).Include(m => m.Penalites)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllMatriculesAsync()
    {
        return await context.Membres.AsNoTracking().Select(m => m.Matricule).ToListAsync();
    }

    public async Task AddAsync(Membre membre)
    {
        await context.Membres.AddAsync(membre);
    }
}
