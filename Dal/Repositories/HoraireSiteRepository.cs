using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class HoraireSiteRepository(PadelDbContext context) : IHoraireSiteRepository
{
    public async Task<HoraireSite?> GetByIdAsync(int id)
    {
        return await context.HorairesSites.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<HoraireSite?> GetBySiteAndYearAsync(int siteId, int annee)
    {
        return await context.HorairesSites.AsNoTracking()
            .FirstOrDefaultAsync(h => h.SiteId == siteId && h.Annee == annee);
    }

    public async Task<IEnumerable<HoraireSite>> GetBySiteIdAsync(int siteId)
    {
        return await context.HorairesSites.AsNoTracking().Where(h => h.SiteId == siteId).ToListAsync();
    }

    public async Task AddAsync(HoraireSite horaireSite)
    {
        await context.HorairesSites.AddAsync(horaireSite);
    }

    public void Update(HoraireSite horaireSite)
    {
        context.HorairesSites.Update(horaireSite);
    }

    public void Delete(HoraireSite horaireSite)
    {
        context.HorairesSites.Remove(horaireSite);
    }
}
