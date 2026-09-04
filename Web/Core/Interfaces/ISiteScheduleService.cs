using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface ISiteScheduleService
{
    Task<SiteScheduleDto> GetBySiteAndYearAsync(int siteId, int year);
    Task<SiteScheduleDto> CreateAsync(int siteId, CreateSiteScheduleDto dto);
}
