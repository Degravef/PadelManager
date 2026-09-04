using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-ETA-005: a match whose date/end time have passed is considered played and no longer modifiable.</summary>
public static class MatchModifiableRule
{
    public static bool IsModifiable(Match match, DateTime now) =>
        match.Status != MatchStatus.Cancelled && match.Date.ToDateTime(match.EndTime) > now;
}
