using Core.Domain.Enums;
using Core.Interfaces;

namespace Core.Domain.Entities;

public class Participation : IConcurrencyToken
{
    public int Id { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public int? MemberId { get; set; }
    public Member? Member { get; set; }
    public required int SeatNumber { get; set; }
    public ParticipationRole Role { get; set; } = ParticipationRole.Player;
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Available;
    public decimal AmountDue { get; set; } = 15m;
    public DateTime RegistrationDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];

    public int Version { get; set; }
}
