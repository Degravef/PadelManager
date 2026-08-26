using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>
/// CF-RV-004/005/006 (simplified — see ASSUMPTION in ReservationService.CreerReservationAsync):
/// a court slot is available if no other active match already occupies that exact start time.
/// </summary>
public static class CreneauDisponibleRule
{
    public static bool EstDisponible(IEnumerable<Match> matchsMemeJourMemeTerrain, TimeOnly heureDebut) =>
        matchsMemeJourMemeTerrain
            .Where(m => m.Statut != StatutMatch.Cancelled)
            .All(m => m.StartTime != heureDebut);
}
