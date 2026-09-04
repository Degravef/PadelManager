using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IParticipationService
{
    Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId);
    Task<ParticipationDto> AddPlayerToPrivateMatchAsync(string organizerMatricule, int matchId, AddPlayerDto dto);
    Task<ParticipationDto> JoinPublicMatchAsync(string matricule, int matchId);
}
