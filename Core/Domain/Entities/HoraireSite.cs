namespace Core.Domain.Entities;

// Opening hours for a Site, defined per calendar year. Unique on (SiteId, Annee).
public class HoraireSite
{
    public int Id { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    public required int Annee { get; set; }
    public TimeOnly HeurePremiereReservation { get; set; }
    public TimeOnly HeureDerniereReservation { get; set; }
    public int DureeMatchMinutes { get; set; } = 90;
    public int PauseMinutes { get; set; } = 15;
    public int NbJoueursRequis { get; set; } = 4;
    public decimal PrixMatch { get; set; } = 60m;
    public ICollection<Creneau> Creneaux { get; set; } = [];
}
