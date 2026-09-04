using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Bll.Services;

// RG-PAY-009 / CF-RC-004.
public class StatisticsService(IPaymentRepository paymentRepository) : IStatisticsService
{
    public async Task<RevenueDto> CalculateRevenueAsync(IEnumerable<int> siteIds, DateOnly start, DateOnly end)
    {
        var payments = await paymentRepository.GetValidatedBySitesAndPeriodAsync(siteIds, start, end);
        decimal amount = payments.Sum(p => p.Amount);
        return new RevenueDto(amount, start, end);
    }
}
