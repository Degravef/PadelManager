using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IParticipationService
{
    Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId);
    Task<ParticipationDto> AddPlayerToPrivateMatchAsync(int matchId, AddPlayerDto dto);
    Task<ParticipationDto> JoinPublicMatchAsync(int matchId);
}
