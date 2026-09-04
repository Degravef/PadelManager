using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface ICourtService
{
    Task<CourtDto> GetCourtByIdAsync(int id);
    Task<IEnumerable<CourtDto>> GetAllCourtsAsync(int? siteId = null);
    Task<CourtDto> CreateCourtAsync(CreateCourtDto dto);
    Task UpdateCourtAsync(int id, UpdateCourtDto dto);
    Task DeleteCourtAsync(int id);
}
