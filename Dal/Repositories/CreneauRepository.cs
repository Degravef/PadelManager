using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class CreneauRepository(PadelDbContext context) : ICreneauRepository
{
    public async Task<Creneau?> GetByIdAsync(int id)
    {
        return await context.Creneaux.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Creneau>> GetByHoraireSiteIdAsync(int horaireSiteId)
    {
        return await context.Creneaux.AsNoTracking()
            .Where(c => c.HoraireSiteId == horaireSiteId)
            .OrderBy(c => c.Ordre)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Creneau> creneaux)
    {
        await context.Creneaux.AddRangeAsync(creneaux);
    }
}
