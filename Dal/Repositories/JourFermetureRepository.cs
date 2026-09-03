using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class JourFermetureRepository(PadelDbContext context) : IJourFermetureRepository
{
    public async Task<JourFermeture?> GetByIdAsync(int id)
    {
        return await context.JoursFermeture.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JourFermeture>> GetBySiteIdAsync(int siteId)
    {
        return await context.JoursFermeture.AsNoTracking().Where(j => j.SiteId == siteId).ToListAsync();
    }

    public async Task<IEnumerable<JourFermeture>> GetGlobalAsync()
    {
        return await context.JoursFermeture.AsNoTracking().Where(j => j.SiteId == null).ToListAsync();
    }

    public async Task AddAsync(JourFermeture jourFermeture)
    {
        await context.JoursFermeture.AddAsync(jourFermeture);
    }

    public void Update(JourFermeture jourFermeture)
    {
        context.JoursFermeture.Update(jourFermeture);
    }

    public void Delete(JourFermeture jourFermeture)
    {
        context.JoursFermeture.Remove(jourFermeture);
    }
}
