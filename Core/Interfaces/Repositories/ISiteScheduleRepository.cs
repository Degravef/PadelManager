using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ISiteScheduleRepository
{
    Task<SiteSchedule?> GetByIdAsync(int id);
    Task<SiteSchedule?> GetBySiteAndYearAsync(int siteId, int year);
    Task<IEnumerable<SiteSchedule>> GetBySiteIdAsync(int siteId);
    Task AddAsync(SiteSchedule siteSchedule);
    void Update(SiteSchedule siteSchedule);
    void Delete(SiteSchedule siteSchedule);
}
