using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class PaymentRepository(PadelDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetByMemberIdAsync(int memberId)
    {
        return await context.Payments.AsNoTracking().Where(p => p.MemberId == memberId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByParticipationIdAsync(int participationId)
    {
        return await context.Payments.AsNoTracking()
            .Where(p => p.ParticipationId == participationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetValidatedBySitesAndPeriodAsync(IEnumerable<int> siteIds, DateOnly start, DateOnly end)
    {
        List<int> ids = siteIds.ToList();

        IQueryable<Payment> viaParticipation = context.Payments.AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Validated && p.ParticipationId != null)
            .Where(p => ids.Contains(p.Participation!.Match!.Court!.SiteId) &&
                        p.Participation!.Match!.Date >= start && p.Participation!.Match!.Date <= end);

        IQueryable<Payment> viaBalanceDue = context.Payments.AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Validated && p.BalanceDueId != null)
            .Where(p => ids.Contains(p.BalanceDue!.Match!.Court!.SiteId) &&
                        p.BalanceDue!.Match!.Date >= start && p.BalanceDue!.Match!.Date <= end);

        return await viaParticipation.Union(viaBalanceDue).ToListAsync();
    }

    public async Task AddAsync(Payment payment)
    {
        await context.Payments.AddAsync(payment);
    }

    public void Update(Payment payment)
    {
        context.Payments.Update(payment);
    }
}
