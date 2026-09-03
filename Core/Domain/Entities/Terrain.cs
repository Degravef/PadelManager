namespace Core.Domain.Entities;

public class Terrain
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    
    
    
    
    public string? Numero { get; set; }
    public string? TypeSurface { get; set; }
    public bool Couvert { get; set; }
    public bool Actif { get; set; } = true;
    public ICollection<Match> Matches { get; set; } = [];
}
