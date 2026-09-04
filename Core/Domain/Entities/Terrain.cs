namespace Core.Domain.Entities;

public class Terrain
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    // ASSUMPTION: nullable (ERD shows NOT NULL + unique per site) because CreateTerrainDto collects it
    // as optional — a real default would collide on the unique index for every 2nd terrain left blank
    // on the same site. Postgres allows multiple NULLs under a unique index, so leaving it blank stays safe.
    public string? Numero { get; set; }
    public string? TypeSurface { get; set; }
    public bool Couvert { get; set; }
    public bool Actif { get; set; } = true;
    public ICollection<Match> Matches { get; set; } = [];
}
