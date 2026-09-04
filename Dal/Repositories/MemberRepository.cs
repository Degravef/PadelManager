using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class MemberRepository(PadelDbContext context) : IMemberRepository
{
    public async Task<Member?> GetByIdAsync(int id)
    {
        return await context.Members.AsNoTracking()
            .Include(m => m.MemberType).Include(m => m.BalancesDue).Include(m => m.Penalties)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetByMatriculeAsync(string matricule)
    {
        return await context.Members.AsNoTracking()
            .Include(m => m.MemberType).Include(m => m.BalancesDue).Include(m => m.Penalties)
            .FirstOrDefaultAsync(m => m.Matricule == matricule);
    }

    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await context.Members.AsNoTracking()
            .Include(m => m.MemberType).Include(m => m.BalancesDue).Include(m => m.Penalties)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllMatriculesAsync()
    {
        return await context.Members.AsNoTracking().Select(m => m.Matricule).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetMatriculesByPrefixAsync(string prefix)
    {
        return await context.Members.AsNoTracking()
            .Where(m => m.Matricule.StartsWith(prefix))
            .Select(m => m.Matricule)
            .ToListAsync();
    }

    public async Task AddAsync(Member member)
    {
        await context.Members.AddAsync(member);
    }
}
