using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class ChevauchementRuleTests
{
    private static Participation Active(int matchId, DateOnly date, TimeOnly debut, TimeOnly fin) => new()
    {
        MatchId = matchId, NumeroPlace = 1, Statut = StatutParticipation.Reservee,
        Match = new Match
        {
            Id = matchId, TerrainId = 1, OrganisateurId = 1, Date = date, StartTime = debut, EndTime = fin,
            TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open
        }
    };

    [Fact]
    public void EstEnChevauchement_NoOtherParticipations_ReturnsFalse()
    {
        Assert.False(ChevauchementRule.EstEnChevauchement([], matchIdActuel: 99, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)));
    }

    [Fact]
    public void EstEnChevauchement_SameMatchBeingActedOn_IsExcluded()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.False(ChevauchementRule.EstEnChevauchement(participations, matchIdActuel: 5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)));
    }

    [Fact]
    public void EstEnChevauchement_OverlappingTimeRangeOnDifferentMatch_ReturnsTrue()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.True(ChevauchementRule.EstEnChevauchement(participations, matchIdActuel: 99, new DateOnly(2026, 9, 10), new TimeOnly(11, 0), new TimeOnly(12, 30)));
    }

    [Fact]
    public void EstEnChevauchement_BackToBackNoOverlap_ReturnsFalse()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.False(ChevauchementRule.EstEnChevauchement(participations, matchIdActuel: 99, new DateOnly(2026, 9, 10), new TimeOnly(11, 30), new TimeOnly(13, 0)));
    }

    [Fact]
    public void EstEnChevauchement_DifferentDate_ReturnsFalse()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.False(ChevauchementRule.EstEnChevauchement(participations, matchIdActuel: 99, new DateOnly(2026, 9, 11), new TimeOnly(10, 0), new TimeOnly(11, 30)));
    }
}
