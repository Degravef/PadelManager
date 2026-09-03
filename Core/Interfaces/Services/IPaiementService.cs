using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IPaiementService
{
    Task<PaiementDto> PayerParticipationAsync(string matricule, int participationId, PayerDto dto);
    Task<PaiementDto> PayerSoldeAsync(string matricule, int soldeDuId, PayerDto dto);
    Task<IEnumerable<SoldeDuDto>> GetMesSoldesImpayesAsync(string matricule);
}
