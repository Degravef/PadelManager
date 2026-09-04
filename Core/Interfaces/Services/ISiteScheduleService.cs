using Core.Dtos;

namespace Core.Interfaces.Services;

public interface ISiteScheduleService
{
    Task<SiteScheduleDto> GetBySiteAndYearAsync(int adminId, int siteId, int year);
    Task<SiteScheduleDto> CreateAsync(int adminId, int siteId, CreateSiteScheduleDto dto);
}
