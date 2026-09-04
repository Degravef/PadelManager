using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> PayParticipationAsync(int participationId, PayDto dto);
    Task<PaymentDto> PayBalanceDueAsync(int balanceDueId, PayDto dto);
    Task<IEnumerable<BalanceDueDto>> GetMyUnpaidBalancesAsync();
}
