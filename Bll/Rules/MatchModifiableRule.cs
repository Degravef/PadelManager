using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// RG-ETA-005
public static class MatchModifiableRule
{
    public static bool IsModifiable(Match match, DateTime now) =>
        match.Status != MatchStatus.Cancelled && match.Date.ToDateTime(match.EndTime) > now;
}
