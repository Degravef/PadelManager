using Core.Domain.Entities;

namespace Bll.Rules;




public static class ChevauchementRule
{
    public static bool EstEnChevauchement(
        IEnumerable<Participation> participationsActives, int matchIdActuel, DateOnly date, TimeOnly debut, TimeOnly fin) =>
        participationsActives.Any(p =>
            p.MatchId != matchIdActuel && p.Match is not null && p.Match.Date == date &&
            p.Match.StartTime < fin && debut < p.Match.EndTime);
}
