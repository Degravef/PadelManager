namespace Web.Core.Dtos;

public record CourtDto(
    int Id,
    string Name,
    int SiteId,
    string? Number = null,
    string? SurfaceType = null,
    bool Covered = false,
    bool Active = true);

public record CreateCourtDto(
    string Name,
    int SiteId,
    string? Number = null,
    string? SurfaceType = null,
    bool Covered = false);

public record UpdateCourtDto(
    string Name,
    string? Number = null,
    string? SurfaceType = null,
    bool Covered = false,
    bool Active = true);
