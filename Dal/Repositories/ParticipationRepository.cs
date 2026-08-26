using Core.Domain.Entities;
using Core.Interfaces.Repositories;

namespace Dal.Repositories;

public class ParticipationRepository(PadelDbContext context) : IParticipationRepository
{
    public async Task AddAsync(Participation participation)
    {
        await context.Participations.AddAsync(participation);
    }
}
