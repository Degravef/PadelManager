using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Participation
{
    public int Id { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public int? MembreId { get; set; } 
    public Membre? Membre { get; set; }
    public required int NumeroPlace { get; set; } 
    public RoleParticipation Role { get; set; } = RoleParticipation.Joueur;
    public StatutParticipation Statut { get; set; } = StatutParticipation.Libre;
    public decimal MontantDu { get; set; } = 15m;
    public DateTime DateInscription { get; set; }
    public DateTime? DateValidation { get; set; } 
    public ICollection<Paiement> Paiements { get; set; } = [];
}
