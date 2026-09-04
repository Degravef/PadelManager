using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public required int MemberId { get; set; }
    public Member? Member { get; set; }
    public int? ParticipationId { get; set; }
    public Participation? Participation { get; set; }
    public int? BalanceDueId { get; set; }
    public BalanceDue? BalanceDue { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; set; }
}
