using Core.Domain.Enums;
using Core.Interfaces;
using MatchType = Core.Domain.Enums.MatchType;

namespace Core.Domain.Entities;

public class Match : IConcurrencyToken
{
    public int Id { get; set; }
    public required int CourtId { get; set; }
    public Court? Court { get; set; }
    public int? SlotId { get; set; }
    public Slot? Slot { get; set; }
    public required int OrganizerId { get; set; }
    public Member? Organizer { get; set; }
    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public required MatchType Type { get; set; }
    public required MatchStatus Status { get; set; }
    public decimal TotalAmount { get; set; } = 60m;
    public decimal AmountPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublicSwitchDate { get; set; }
    public DateOnly PaymentDeadline { get; set; }
    public ICollection<Participation> Participations { get; set; } = [];
    public ICollection<BalanceDue> BalancesDue { get; set; } = [];
    public ICollection<Penalty> Penalties { get; set; } = [];

    public int Version { get; set; }
}
