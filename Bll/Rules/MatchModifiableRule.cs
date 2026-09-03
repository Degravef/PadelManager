using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;


public static class MatchModifiableRule
{
    public static bool EstModifiable(Match match, DateTime maintenant) =>
        match.Statut != StatutMatch.Cancelled && match.Date.ToDateTime(match.EndTime) > maintenant;
}
