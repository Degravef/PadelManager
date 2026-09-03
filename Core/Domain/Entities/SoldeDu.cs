using Core.Domain.Enums;

namespace Core.Domain.Entities;

// CF-RC-003 / CF-RV-015/016/017: balance an organizer owes when a match doesn't recoup its 60€ cost.
public class SoldeDu
{
    public int Id { get; set; }
    public required int MembreId { get; set; } // organisateur redevable
    public Membre? Membre { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public decimal Montant { get; set; }
    public StatutSoldeDu Statut { get; set; } = StatutSoldeDu.Du;
    public DateTime DateCreation { get; set; }
    public DateTime? DateReglement { get; set; }
    public ICollection<Paiement> Paiements { get; set; } = [];
}
