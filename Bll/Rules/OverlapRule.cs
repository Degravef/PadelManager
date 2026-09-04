using Core.Domain.Entities;

namespace Bll.Rules;

/// RG-ETA-006
public static class OverlapRule
{
    public static bool IsOverlapping(
        IEnumerable<Participation> activeParticipations, int currentMatchId, DateOnly date, TimeOnly start, TimeOnly end) =>
        activeParticipations.Any(p =>
            p.MatchId != currentMatchId && p.Match is not null && p.Match.Date == date &&
            p.Match.StartTime < end && start < p.Match.EndTime);
}
