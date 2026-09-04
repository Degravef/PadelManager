using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByMemberIdAsync(int memberId);
    Task<IEnumerable<Payment>> GetByParticipationIdAsync(int participationId);

    /// RG-PAY-009 / CF-RC-004
    Task<IEnumerable<Payment>> GetValidatedBySitesAndPeriodAsync(IEnumerable<int> siteIds, DateOnly start, DateOnly end);
    Task AddAsync(Payment payment);
    void Update(Payment payment);
}
