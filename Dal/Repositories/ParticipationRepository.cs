using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class ParticipationRepository(PadelDbContext context) : IParticipationRepository
{
    public async Task<Participation?> GetByIdAsync(int id)
    {
        return await context.Participations.AsNoTracking()
            .Include(p => p.Match)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Participation>> GetByMatchIdAsync(int matchId)
    {
        return await context.Participations.AsNoTracking().Where(p => p.MatchId == matchId).ToListAsync();
    }

    public async Task<IEnumerable<Participation>> GetActiveByMemberIdAsync(int memberId)
    {
        return await context.Participations.AsNoTracking()
            .Include(p => p.Match)
            .Where(p => p.MemberId == memberId && (p.Status == ParticipationStatus.Reserved || p.Status == ParticipationStatus.Paid))
            .ToListAsync();
    }

    public async Task AddAsync(Participation participation)
    {
        await context.Participations.AddAsync(participation);
    }

    public void Update(Participation participation)
    {
        context.Participations.Update(participation);
    }

    public void Delete(Participation participation)
    {
        context.Participations.Remove(participation);
    }
}
