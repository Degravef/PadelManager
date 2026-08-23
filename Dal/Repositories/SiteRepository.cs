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

    public async Task<IEnumerable<Site>> GetAllAsync()
    {
        return await context.Sites.ToListAsync();
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

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await context.Sites.AnyAsync(s => s.Name == name);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}