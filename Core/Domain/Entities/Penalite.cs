namespace Core.Domain.Entities;

// CF-RV-008/009: 1-week reservation penalty applied when a private match stays incomplete.
public class Penalite
{
    public int Id { get; set; }
    public required int MembreId { get; set; }
    public Membre? Membre { get; set; }
    public int? MatchId { get; set; } // match a l'origine de la penalite
    public Match? Match { get; set; }
    public string Motif { get; set; } = string.Empty;
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; } // date_debut + 7 jours
    public bool Active { get; set; } = true;
}
