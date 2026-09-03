using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IHoraireSiteService
{
    Task<HoraireSiteDto> GetBySiteAndYearAsync(int siteId, int annee);
    Task<HoraireSiteDto> CreateAsync(int siteId, CreateHoraireSiteDto dto);
}
