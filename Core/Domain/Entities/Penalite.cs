namespace Core.Domain.Entities;


public class Penalite
{
    public int Id { get; set; }
    public required int MembreId { get; set; }
    public Membre? Membre { get; set; }
    public int? MatchId { get; set; } 
    public Match? Match { get; set; }
    public string Motif { get; set; } = string.Empty;
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; } 
    public bool Active { get; set; } = true;
}
