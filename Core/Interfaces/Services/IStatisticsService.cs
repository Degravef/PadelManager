using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IStatisticsService
{
    // RG-PAY-009 / CF-RC-004. Caller resolves which site ids it's allowed to see (e.g. an admin's own sites).
    Task<RevenueDto> CalculateRevenueAsync(IEnumerable<int> siteIds, DateOnly start, DateOnly end);
}
