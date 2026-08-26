using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface ITerrainService
{
    Task<TerrainDto> GetTerrainByIdAsync(int id);
    Task<IEnumerable<TerrainDto>> GetAllTerrainsAsync(int? siteId = null);
    Task<TerrainDto> CreateTerrainAsync(CreateTerrainDto dto);
    Task UpdateTerrainAsync(int id, UpdateTerrainDto dto);
    Task DeleteTerrainAsync(int id);
}
