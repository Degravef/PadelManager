namespace Core.Domain.Entities;

// A mechanically-derived slot (90 + 15 min) for a HoraireSite. Unique on (HoraireSiteId, Ordre).
public class Creneau
{
    public int Id { get; set; }
    public required int HoraireSiteId { get; set; }
    public HoraireSite? HoraireSite { get; set; }
    public required int Ordre { get; set; } // 1, 2, 3 ...
    public TimeOnly HeureDebut { get; set; }
    public TimeOnly HeureFin { get; set; }
    public ICollection<Match> Matches { get; set; } = [];
}
