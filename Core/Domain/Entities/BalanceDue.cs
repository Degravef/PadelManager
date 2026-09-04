using Core.Domain.Enums;

namespace Core.Domain.Entities;

// CF-RC-003 / CF-RV-015/016/017: balance an organizer owes when a match doesn't recoup its 60€ cost.
public class BalanceDue
{
    public int Id { get; set; }
    public required int MemberId { get; set; } // organizer owing the balance
    public Member? Member { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public decimal Amount { get; set; }
    public BalanceDueStatus Status { get; set; } = BalanceDueStatus.Due;
    public DateTime CreatedAt { get; set; }
    public DateTime? SettlementDate { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];
}
