namespace Core.Dtos;

public record TerrainDto(
    int Id,
    string Name,
    int SiteId,
    string? Numero = null,
    string? TypeSurface = null,
    bool Couvert = false,
    bool Actif = true);

public record CreateTerrainDto(
    string Name,
    int SiteId,
    string? Numero = null,
    string? TypeSurface = null,
    bool Couvert = false);

public record UpdateTerrainDto(
    string Name,
    string? Numero = null,
    string? TypeSurface = null,
    bool Couvert = false,
    bool Actif = true);
