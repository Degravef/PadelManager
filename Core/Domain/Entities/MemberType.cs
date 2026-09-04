namespace Core.Domain.Entities;

// Reference/rule table (readme.md ERD): booking-window lead time per member type is data, not code.
public class MemberType
{
    public int Id { get; set; }
    public required string Code { get; set; } // GLOBAL / SITE / LIBRE
    public required string Label { get; set; }
    public required string MatriculePrefix { get; set; } // G / S / L
    public required int ReservationWindowDays { get; set; } // 21 / 14 / 5
    public ICollection<Member> Members { get; set; } = [];
}
