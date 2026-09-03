namespace Core.Domain.Entities;


public class Creneau
{
    public int Id { get; set; }
    public required int HoraireSiteId { get; set; }
    public HoraireSite? HoraireSite { get; set; }
    public required int Ordre { get; set; } 
    public TimeOnly HeureDebut { get; set; }
    public TimeOnly HeureFin { get; set; }
    public ICollection<Match> Matches { get; set; } = [];
}
