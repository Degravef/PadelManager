namespace Core.Domain.Entities;

public class Terrain
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    // ASSUMPTION: nullable (ERD shows NOT NULL + unique per site) because no Service populates it yet
    // (CreateTerrainDto has no Numero field) — a real default would collide on the unique index for
    // every 2nd terrain on the same site. Postgres allows multiple NULLs under a unique index, so this
    // stays safe until terrain creation is extended to collect it.
    public string? Numero { get; set; }
    public string? TypeSurface { get; set; }
    public bool Couvert { get; set; }
    public bool Actif { get; set; } = true;
    public ICollection<Match> Matches { get; set; } = [];
}
