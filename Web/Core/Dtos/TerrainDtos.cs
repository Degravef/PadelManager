namespace Web.Core.Dtos;

public record TerrainDto(int Id, string Name, int SiteId);

public record CreateTerrainDto(string Name, int SiteId);

public record UpdateTerrainDto(string Name);
