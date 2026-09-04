using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using MatchType = Core.Domain.Enums.MatchType;

namespace BllTest.Rules;

public class MatchModifiableRuleTests
{
    private static Match Match(DateOnly date, TimeOnly endTime, MatchStatus status = MatchStatus.Open) => new()
    {
        CourtId = 1, OrganizerId = 1, Date = date, StartTime = new TimeOnly(10, 0), EndTime = endTime,
        Type = MatchType.Private, Status = status
    };

    [Fact]
    public void IsModifiable_EndTimeInTheFuture_ReturnsTrue()
    {
        var match = Match(new DateOnly(2026, 9, 10), new TimeOnly(11, 30));

        Assert.True(MatchModifiableRule.IsModifiable(match, new DateTime(2026, 9, 10, 9, 0, 0)));
    }

    [Fact]
    public void IsModifiable_EndTimeInThePast_ReturnsFalse()
    {
        var match = Match(new DateOnly(2026, 9, 10), new TimeOnly(11, 30));

        Assert.False(MatchModifiableRule.IsModifiable(match, new DateTime(2026, 9, 10, 12, 0, 0)));
    }

    [Fact]
    public void IsModifiable_CancelledMatch_ReturnsFalse()
    {
        var match = Match(new DateOnly(2026, 9, 10), new TimeOnly(11, 30), MatchStatus.Cancelled);

        Assert.False(MatchModifiableRule.IsModifiable(match, new DateTime(2026, 9, 10, 9, 0, 0)));
    }
}
