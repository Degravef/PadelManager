using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IParticipationRepository
{
    Task AddAsync(Participation participation);
}
