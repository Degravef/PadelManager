namespace Core.Domain.Entities;

public class Court
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    public string? Number { get; set; }
    public string? SurfaceType { get; set; }
    public bool Covered { get; set; }
    public bool Active { get; set; } = true;
    public ICollection<Match> Matches { get; set; } = [];
}
