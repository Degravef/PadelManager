using Core.Domain.Enums;
using Core.Interfaces;

namespace Core.Domain.Entities;

public class Participation : IConcurrencyToken
{
    public int Id { get; set; }
    public required int MatchId { get; set; }
    public Match? Match { get; set; }
    public int? MemberId { get; set; } // NULL = open seat
    public Member? Member { get; set; }
    public required int SeatNumber { get; set; } // 1 to 4, unique with MatchId
    public ParticipationRole Role { get; set; } = ParticipationRole.Player;
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Available;
    public decimal AmountDue { get; set; } = 15m;
    public DateTime RegistrationDate { get; set; }
    public DateTime? PaymentDate { get; set; } // = date of payment
    public ICollection<Payment> Payments { get; set; } = [];

    // Optimistic-concurrency counter (see IConcurrencyToken): protects Status/PaymentDate against
    // a payment racing MatchLifecycleService's daily batch (which deletes an unpaid seat J-1).
    public int Version { get; set; }
}
