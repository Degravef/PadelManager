using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public required int TerrainId { get; set; }
    public Terrain? Terrain { get; set; }
    public int? CreneauId { get; set; }
    public Creneau? Creneau { get; set; }
    public required int OrganisateurId { get; set; }
    public Membre? Organisateur { get; set; }
    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public required TypeMatch TypeMatch { get; set; } // PRIVE / PUBLIC (readme.md ERD's "visibilite")
    public required StatutMatch Statut { get; set; }
    public decimal MontantTotal { get; set; } = 60m;
    public decimal MontantPaye { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateBasculePublic { get; set; } // NULL tant que prive
    public DateOnly DateLimite { get; set; } // veille du match
    public ICollection<Participation> Participations { get; set; } = [];
    public ICollection<SoldeDu> SoldesDus { get; set; } = [];
    public ICollection<Penalite> Penalites { get; set; } = [];
}
