using Core.Dtos;

namespace Core.Interfaces.Services;

public interface ICourtService
{
    Task<CourtDto> GetCourtByIdAsync(int adminId, int id);
    Task<IEnumerable<CourtDto>> GetAllCourtsAsync(int adminId, int? siteId = null);
    Task<IEnumerable<CourtDto>> GetCourtsBySiteAsync(int siteId);
    Task<CourtDto> CreateCourtAsync(int adminId, CreateCourtDto dto);
    Task UpdateCourtAsync(int adminId, int id, UpdateCourtDto dto);
    Task DeleteCourtAsync(int adminId, int id);
}
