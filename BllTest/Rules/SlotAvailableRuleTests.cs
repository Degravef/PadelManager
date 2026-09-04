using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using MatchType = Core.Domain.Enums.MatchType;

namespace BllTest.Rules;

public class SlotAvailableRuleTests
{
    private static Match Existing(TimeOnly startTime, MatchStatus status = MatchStatus.Open) => new()
    {
        CourtId = 1, Date = new DateOnly(2026, 9, 1), StartTime = startTime,
        Type = MatchType.Private, Status = status, OrganizerId = 1
    };

    [Fact]
    public void IsAvailable_NoExistingMatch_ReturnsTrue()
    {
        Assert.True(SlotAvailableRule.IsAvailable([], new TimeOnly(10, 0)));
    }

    [Fact]
    public void IsAvailable_DifferentStartTime_ReturnsTrue()
    {
        var existing = new[] { Existing(new TimeOnly(9, 0)) };

        Assert.True(SlotAvailableRule.IsAvailable(existing, new TimeOnly(10, 0)));
    }

    [Fact]
    public void IsAvailable_SameStartTimeActiveMatch_ReturnsFalse()
    {
        var existing = new[] { Existing(new TimeOnly(10, 0)) };

        Assert.False(SlotAvailableRule.IsAvailable(existing, new TimeOnly(10, 0)));
    }

    [Fact]
    public void IsAvailable_SameStartTimeButCancelledMatch_ReturnsTrue()
    {
        var existing = new[] { Existing(new TimeOnly(10, 0), MatchStatus.Cancelled) };

        Assert.True(SlotAvailableRule.IsAvailable(existing, new TimeOnly(10, 0)));
    }
}
