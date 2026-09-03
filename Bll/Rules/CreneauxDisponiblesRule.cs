using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>
/// RG-SITE-003/004/005: generates the site's valid match start times for one HoraireSite — the first
/// slot starts at opening time, each following slot starts (match duration + buffer) later, and the
/// last slot's start time may not be later than the site's last-reservation hour.
/// </summary>
public static class CreneauxDisponiblesRule
{
    public static IReadOnlyList<TimeOnly> Calculer(HoraireSite horaire)
    {
        var creneaux = new List<TimeOnly>();
        TimeSpan pas = TimeSpan.FromMinutes(horaire.DureeMatchMinutes + horaire.PauseMinutes);
        TimeOnly debut = horaire.HeurePremiereReservation;

        while (debut <= horaire.HeureDerniereReservation)
        {
            creneaux.Add(debut);
            TimeOnly suivant = debut.Add(pas);
            if (suivant <= debut)
                break; // wrapped past midnight — no further slots fit in the day
            debut = suivant;
        }

        return creneaux;
    }
}
