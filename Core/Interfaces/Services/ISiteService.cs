using Core.Dtos;

namespace Core.Interfaces.Services;

public interface ISiteService
{
    Task<SiteDto> GetSiteByIdAsync(int adminId, int id); // ne renvoie plus null, lève NotFoundException
    Task<IEnumerable<SiteDto>> GetAllSitesAsync(int adminId);
    Task<SiteDto> CreateSiteAsync(int adminId, CreateSiteDto dto);
    Task UpdateSiteAsync(int adminId, int id, UpdateSiteDto dto);
    Task DeleteSiteAsync(int adminId, int id);
}