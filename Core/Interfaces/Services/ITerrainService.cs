using Core.Dtos;

namespace Core.Interfaces.Services;

public interface ITerrainService
{
    Task<TerrainDto> GetTerrainByIdAsync(int adminId, int id);
    Task<IEnumerable<TerrainDto>> GetAllTerrainsAsync(int adminId, int? siteId = null);
    Task<TerrainDto> CreateTerrainAsync(int adminId, CreateTerrainDto dto);
    Task UpdateTerrainAsync(int adminId, int id, UpdateTerrainDto dto);
    Task DeleteTerrainAsync(int adminId, int id);
}
