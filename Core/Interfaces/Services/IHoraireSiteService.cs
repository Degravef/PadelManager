using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IHoraireSiteService
{
    Task<HoraireSiteDto> GetBySiteAndYearAsync(int adminId, int siteId, int annee);
    Task<HoraireSiteDto> CreateAsync(int adminId, int siteId, CreateHoraireSiteDto dto);
}
