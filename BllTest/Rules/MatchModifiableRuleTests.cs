using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class MatchModifiableRuleTests
{
    private static Match Match(DateOnly date, TimeOnly endTime, StatutMatch statut = StatutMatch.Open) => new()
    {
        TerrainId = 1, OrganisateurId = 1, Date = date, StartTime = new TimeOnly(10, 0), EndTime = endTime,
        TypeMatch = TypeMatch.Private, Statut = statut
    };

    [Fact]
    public void EstModifiable_EndTimeInTheFuture_ReturnsTrue()
    {
        var match = Match(new DateOnly(2026, 9, 10), new TimeOnly(11, 30));

        Assert.True(MatchModifiableRule.EstModifiable(match, new DateTime(2026, 9, 10, 9, 0, 0)));
    }

    [Fact]
    public void EstModifiable_EndTimeInThePast_ReturnsFalse()
    {
        var match = Match(new DateOnly(2026, 9, 10), new TimeOnly(11, 30));

        Assert.False(MatchModifiableRule.EstModifiable(match, new DateTime(2026, 9, 10, 12, 0, 0)));
    }

    [Fact]
    public void EstModifiable_CancelledMatch_ReturnsFalse()
    {
        var match = Match(new DateOnly(2026, 9, 10), new TimeOnly(11, 30), StatutMatch.Cancelled);

        Assert.False(MatchModifiableRule.EstModifiable(match, new DateTime(2026, 9, 10, 9, 0, 0)));
    }
}
