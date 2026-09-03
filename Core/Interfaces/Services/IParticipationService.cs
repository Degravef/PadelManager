using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IParticipationService
{
    Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId);
    Task<ParticipationDto> AjouterJoueurMatchPriveAsync(string matriculeOrganisateur, int matchId, AjouterJoueurDto dto);
    Task<ParticipationDto> RejoindreMatchPublicAsync(string matricule, int matchId);
}
