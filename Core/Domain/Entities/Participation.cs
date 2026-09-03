using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Participation
{
    public int Id { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public int? MembreId { get; set; } // NULL = place libre
    public Membre? Membre { get; set; }
    public required int NumeroPlace { get; set; } // 1 a 4, unique avec MatchId
    public RoleParticipation Role { get; set; } = RoleParticipation.Joueur;
    public StatutParticipation Statut { get; set; } = StatutParticipation.Libre;
    public decimal MontantDu { get; set; } = 15m;
    public DateTime DateInscription { get; set; }
    public DateTime? DateValidation { get; set; } // = date du paiement
    public ICollection<Paiement> Paiements { get; set; } = [];
}
