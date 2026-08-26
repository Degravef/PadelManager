using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface ISiteService
{
    Task<SiteDto> GetSiteByIdAsync(int id);
    Task<IEnumerable<SiteDto>> GetAllSitesAsync();
    Task<SiteDto> CreateSiteAsync(CreateSiteDto dto);
    Task UpdateSiteAsync(int id, UpdateSiteDto dto);
    Task DeleteSiteAsync(int id);
}