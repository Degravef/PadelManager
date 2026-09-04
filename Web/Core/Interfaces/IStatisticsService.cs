using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IStatisticsService
{
    Task<RevenueDto> CalculateRevenueAsync(int? siteId, DateOnly start, DateOnly end);
}
