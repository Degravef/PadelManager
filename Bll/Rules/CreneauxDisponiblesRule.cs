using Core.Domain.Entities;

namespace Bll.Rules;






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
                break; 
            debut = suivant;
        }

        return creneaux;
    }
}
