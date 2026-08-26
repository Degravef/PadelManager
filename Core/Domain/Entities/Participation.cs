namespace Core.Domain.Entities;

public class Participation
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public required int MembreId { get; set; }
    public Membre? Membre { get; set; }
    public bool APaye { get; set; }
    public DateTime? DatePaiement { get; set; }
    public decimal MontantDu { get; set; }
    public DateTime DateInscription { get; set; }
}
