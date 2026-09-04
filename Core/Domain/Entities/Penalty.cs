namespace Core.Domain.Entities;

/// CF-RV-008/009
public class Penalty
{
    public int Id { get; set; }
    public required int MemberId { get; set; }
    public Member? Member { get; set; }
    public int? MatchId { get; set; }
    public Match? Match { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool Active { get; set; } = true;
}
