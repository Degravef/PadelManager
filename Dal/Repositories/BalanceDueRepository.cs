using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class BalanceDueRepository(PadelDbContext context) : IBalanceDueRepository
{
    public async Task<BalanceDue?> GetByIdAsync(int id)
    {
        return await context.BalancesDue.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<BalanceDue>> GetByMemberIdAsync(int memberId)
    {
        return await context.BalancesDue.AsNoTracking().Where(s => s.MemberId == memberId).ToListAsync();
    }

    public async Task<IEnumerable<BalanceDue>> GetOutstandingByMemberIdAsync(int memberId)
    {
        return await context.BalancesDue.AsNoTracking()
            .Where(s => s.MemberId == memberId && s.Status == BalanceDueStatus.Due)
            .ToListAsync();
    }

    public async Task<BalanceDue?> GetByMatchIdAsync(int matchId)
    {
        return await context.BalancesDue.AsNoTracking().FirstOrDefaultAsync(s => s.MatchId == matchId);
    }

    public async Task AddAsync(BalanceDue balanceDue)
    {
        await context.BalancesDue.AddAsync(balanceDue);
    }

    public void Update(BalanceDue balanceDue)
    {
        context.BalancesDue.Update(balanceDue);
    }
}
