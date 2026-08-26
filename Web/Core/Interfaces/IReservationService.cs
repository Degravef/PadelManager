using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IReservationService
{
    Task<MatchDto> GetReservationByIdAsync(int id);
    Task<IEnumerable<MatchDto>> GetMyReservationsAsync();
    Task<MatchDto> CreerReservationAsync(CreerReservationDto dto);
    Task<IEnumerable<SiteDto>> GetSitesAsync();
    Task<IEnumerable<TerrainDto>> GetTerrainsBySiteAsync(int siteId);
}
