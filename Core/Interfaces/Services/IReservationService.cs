using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IReservationService
{
    Task<MatchDto> GetReservationByIdAsync(string matricule, int id);
    Task<IEnumerable<MatchDto>> GetMyReservationsAsync(string matricule);
    Task<MatchDto> CreateReservationAsync(string matricule, CreateReservationDto dto);
    Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int siteId, DateOnly date);
}
