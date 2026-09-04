using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IReservationService
{
    Task<MatchDto> GetReservationByIdAsync(int id);
    Task<IEnumerable<MatchDto>> GetMyReservationsAsync();
    Task<MatchDto> CreateReservationAsync(CreateReservationDto dto);
    Task<IEnumerable<SiteDto>> GetSitesAsync();
    Task<IEnumerable<CourtDto>> GetCourtsBySiteAsync(int siteId);
    Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int siteId, DateOnly date);
}
