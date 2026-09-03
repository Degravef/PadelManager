using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Paiement
{
    public int Id { get; set; }
    public required int MembreId { get; set; }
    public Membre? Membre { get; set; }
    public int? ParticipationId { get; set; } 
    public Participation? Participation { get; set; }
    public int? SoldeDuId { get; set; } 
    public SoldeDu? SoldeDu { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; }
    public string MoyenPaiement { get; set; } = string.Empty;
    public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;
    public string? ReferenceTransaction { get; set; }
}
