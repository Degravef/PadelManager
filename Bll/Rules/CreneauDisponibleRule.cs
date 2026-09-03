using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>
/// RG-SITE-006: a court can only host one match per date/slot — a start time is available if no
/// other active match already occupies that exact start time on the same court and date.
/// </summary>
public static class CreneauDisponibleRule
{
    public static bool EstDisponible(IEnumerable<Match> matchsMemeJourMemeTerrain, TimeOnly heureDebut) =>
        matchsMemeJourMemeTerrain
            .Where(m => m.Statut != StatutMatch.Cancelled)
            .All(m => m.StartTime != heureDebut);
}
