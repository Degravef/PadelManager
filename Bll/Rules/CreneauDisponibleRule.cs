using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;





public static class CreneauDisponibleRule
{
    public static bool EstDisponible(IEnumerable<Match> matchsMemeJourMemeTerrain, TimeOnly heureDebut) =>
        matchsMemeJourMemeTerrain
            .Where(m => m.Statut != StatutMatch.Cancelled)
            .All(m => m.StartTime != heureDebut);
}
