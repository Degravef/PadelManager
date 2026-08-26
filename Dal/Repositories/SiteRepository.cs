using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class SiteRepository(PadelDbContext context) : ISiteRepository
{
    public async Task<Site?> GetByIdAsync(int id)
    {
        return await context.Sites.FindAsync(id);
    }

    public async Task<IEnumerable<Site>> GetByAdminIdAsync(int adminId)
    {
        return await context.Sites.Where(s => s.AdminId == adminId).ToListAsync();
    }

    public async Task<IEnumerable<Site>> GetAllAsync()
    {
        return await context.Sites.ToListAsync();
    }

    public async Task<IEnumerable<int>> GetDistinctAdminIdsAsync()
    {
        return await context.Sites.Select(s => s.AdminId).Distinct().ToListAsync();
    }

    public async Task AddAsync(Site site)
    {
        await context.Sites.AddAsync(site);
    }

    public void Update(Site site)
    {
        context.Sites.Update(site);
    }

    public void Delete(Site site)
    {
        context.Sites.Remove(site);
    }
}