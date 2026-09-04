using Core.Dtos;

namespace Core.Interfaces.Services;

public interface ISiteService
{
    Task<SiteDto> GetSiteByIdAsync(int adminId, int id); // no longer returns null, throws NotFoundException
    Task<IEnumerable<SiteDto>> GetAllSitesAsync(int adminId);
    Task<IEnumerable<SiteDto>> GetAllSitesPublicAsync();
    Task<IEnumerable<int>> GetAllAdminIdsAsync();
    Task<SiteDto> CreateSiteAsync(int adminId, CreateSiteDto dto);
    Task UpdateSiteAsync(int adminId, int id, UpdateSiteDto dto);
    Task DeleteSiteAsync(int adminId, int id);
}
