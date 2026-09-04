using Core.Domain.Enums;
using Core.Interfaces;

namespace Core.Domain.Entities;

/// CF-RC-003 / CF-RV-015/016/017
public class BalanceDue : IConcurrencyToken
{
    public int Id { get; set; }
    public required int MemberId { get; set; }
    public Member? Member { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public decimal Amount { get; set; }
    public BalanceDueStatus Status { get; set; } = BalanceDueStatus.Due;
    public DateTime CreatedAt { get; set; }
    public DateTime? SettlementDate { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];

    public int Version { get; set; }
}
