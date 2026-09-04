namespace Core.Domain.Entities;

public class Court
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    // ASSUMPTION: nullable (ERD shows NOT NULL + unique per site) because CreateCourtDto collects it
    // as optional — a real default would collide on the unique index for every 2nd court left blank
    // on the same site. Postgres allows multiple NULLs under a unique index, so leaving it blank stays safe.
    public string? Number { get; set; }
    public string? SurfaceType { get; set; }
    public bool Covered { get; set; }
    public bool Active { get; set; } = true;
    public ICollection<Match> Matches { get; set; } = [];
}
