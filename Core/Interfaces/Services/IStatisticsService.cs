using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IStatisticsService
{
    /// RG-PAY-009 / CF-RC-004
    Task<RevenueDto> CalculateRevenueAsync(IEnumerable<int> siteIds, DateOnly start, DateOnly end);
}
