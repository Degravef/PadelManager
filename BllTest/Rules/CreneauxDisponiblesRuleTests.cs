using Bll.Rules;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class CreneauxDisponiblesRuleTests
{
    private static HoraireSite Horaire(TimeOnly debut, TimeOnly fin, int dureeMinutes = 90, int pauseMinutes = 15) => new()
    {
        SiteId = 1, Annee = 2026, HeurePremiereReservation = debut, HeureDerniereReservation = fin,
        DureeMatchMinutes = dureeMinutes, PauseMinutes = pauseMinutes
    };

    [Fact]
    public void Calculer_WideWindow_GeneratesSlotsEvery1h45ApartStartingAtOpeningTime()
    {
        var slots = CreneauxDisponiblesRule.Calculer(Horaire(new TimeOnly(8, 0), new TimeOnly(21, 0)));

        Assert.Equal(
        [
            new TimeOnly(8, 0), new TimeOnly(9, 45), new TimeOnly(11, 30), new TimeOnly(13, 15),
            new TimeOnly(15, 0), new TimeOnly(16, 45), new TimeOnly(18, 30), new TimeOnly(20, 15)
        ], slots);
    }

    [Fact]
    public void Calculer_LastSlotStartExactlyOnClosingHour_IsIncluded()
    {
        var slots = CreneauxDisponiblesRule.Calculer(Horaire(new TimeOnly(8, 0), new TimeOnly(9, 45)));

        Assert.Equal([new TimeOnly(8, 0), new TimeOnly(9, 45)], slots);
    }

    [Fact]
    public void Calculer_WindowNarrowerThanOneSlot_ReturnsOnlyOpeningSlot()
    {
        var slots = CreneauxDisponiblesRule.Calculer(Horaire(new TimeOnly(8, 0), new TimeOnly(8, 30)));

        Assert.Equal([new TimeOnly(8, 0)], slots);
    }
}
