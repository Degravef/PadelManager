using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// RG-SITE-006
public static class SlotAvailableRule
{
    public static bool IsAvailable(IEnumerable<Match> matchesSameDaySameCourt, TimeOnly startTime) =>
        matchesSameDaySameCourt
            .Where(m => m.Status != MatchStatus.Cancelled)
            .All(m => m.StartTime != startTime);
}
