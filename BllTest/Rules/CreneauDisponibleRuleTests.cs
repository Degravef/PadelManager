using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class CreneauDisponibleRuleTests
{
    private static Match Existing(TimeOnly startTime, StatutMatch statut = StatutMatch.Open) => new()
    {
        TerrainId = 1, Date = new DateOnly(2026, 9, 1), StartTime = startTime,
        TypeMatch = TypeMatch.Private, Statut = statut, OrganisateurId = 1
    };

    [Fact]
    public void EstDisponible_NoExistingMatch_ReturnsTrue()
    {
        Assert.True(CreneauDisponibleRule.EstDisponible([], new TimeOnly(10, 0)));
    }

    [Fact]
    public void EstDisponible_DifferentStartTime_ReturnsTrue()
    {
        var existing = new[] { Existing(new TimeOnly(9, 0)) };

        Assert.True(CreneauDisponibleRule.EstDisponible(existing, new TimeOnly(10, 0)));
    }

    [Fact]
    public void EstDisponible_SameStartTimeActiveMatch_ReturnsFalse()
    {
        var existing = new[] { Existing(new TimeOnly(10, 0)) };

        Assert.False(CreneauDisponibleRule.EstDisponible(existing, new TimeOnly(10, 0)));
    }

    [Fact]
    public void EstDisponible_SameStartTimeButCancelledMatch_ReturnsTrue()
    {
        var existing = new[] { Existing(new TimeOnly(10, 0), StatutMatch.Cancelled) };

        Assert.True(CreneauDisponibleRule.EstDisponible(existing, new TimeOnly(10, 0)));
    }
}
