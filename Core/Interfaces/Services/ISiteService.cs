using Core.Dtos;

namespace Core.Interfaces.Services;

public interface ISiteService
{
    Task<SiteDto?> GetSiteByIdAsync(int id);
    Task<IEnumerable<SiteDto>> GetAllSitesAsync(int adminId);
    Task<SiteDto> CreateSiteAsync(int adminId, CreateSiteDto dto);
    Task UpdateSiteAsync(int id, UpdateSiteDto dto);
    Task DeleteSiteAsync(int id);
}