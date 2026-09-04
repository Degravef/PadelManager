using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class PenaltyRepository(PadelDbContext context) : IPenaltyRepository
{
    public async Task<Penalty?> GetByIdAsync(int id)
    {
        return await context.Penalties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Penalty>> GetByMemberIdAsync(int memberId)
    {
        return await context.Penalties.AsNoTracking().Where(p => p.MemberId == memberId).ToListAsync();
    }

    public async Task<IEnumerable<Penalty>> GetActiveByMemberIdAsync(int memberId)
    {
        return await context.Penalties.AsNoTracking()
            .Where(p => p.MemberId == memberId && p.Active)
            .ToListAsync();
    }

    public async Task AddAsync(Penalty penalty)
    {
        await context.Penalties.AddAsync(penalty);
    }

    public void Update(Penalty penalty)
    {
        context.Penalties.Update(penalty);
    }
}
