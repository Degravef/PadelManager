namespace Core.Domain.Entities;

// CF-RV-008/009: 1-week reservation penalty applied when a private match stays incomplete.
public class Penalty
{
    public int Id { get; set; }
    public required int MemberId { get; set; }
    public Member? Member { get; set; }
    public int? MatchId { get; set; } // match that triggered the penalty
    public Match? Match { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; } // start_date + 7 days
    public bool Active { get; set; } = true;
}
