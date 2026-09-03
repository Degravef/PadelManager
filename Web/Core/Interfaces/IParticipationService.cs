using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IParticipationService
{
    Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId);
    Task<ParticipationDto> AjouterJoueurAsync(int matchId, AjouterJoueurDto dto);
    Task<ParticipationDto> RejoindreAsync(int matchId);
}
