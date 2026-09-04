using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentDto> PayParticipationAsync(string matricule, int participationId, PayDto dto);
    Task<PaymentDto> PayBalanceDueAsync(string matricule, int balanceDueId, PayDto dto);
    Task<IEnumerable<BalanceDueDto>> GetMyUnpaidBalancesAsync(string matricule);
}
