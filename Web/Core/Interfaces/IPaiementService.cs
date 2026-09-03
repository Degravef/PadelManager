using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IPaiementService
{
    Task<PaiementDto> PayerParticipationAsync(int participationId, PayerDto dto);
    Task<PaiementDto> PayerSoldeAsync(int soldeDuId, PayerDto dto);
    Task<IEnumerable<SoldeDuDto>> GetMesSoldesAsync();
}
