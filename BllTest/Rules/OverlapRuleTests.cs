using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using MatchType = Core.Domain.Enums.MatchType;

namespace BllTest.Rules;

public class OverlapRuleTests
{
    private static Participation Active(int matchId, DateOnly date, TimeOnly start, TimeOnly end) => new()
    {
        MatchId = matchId, SeatNumber = 1, Status = ParticipationStatus.Reserved,
        Match = new Match
        {
            Id = matchId, CourtId = 1, OrganizerId = 1, Date = date, StartTime = start, EndTime = end,
            Type = MatchType.Private, Status = MatchStatus.Open
        }
    };

    [Fact]
    public void IsOverlapping_NoOtherParticipations_ReturnsFalse()
    {
        Assert.False(OverlapRule.IsOverlapping([], currentMatchId: 99, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)));
    }

    [Fact]
    public void IsOverlapping_SameMatchBeingActedOn_IsExcluded()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.False(OverlapRule.IsOverlapping(participations, currentMatchId: 5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)));
    }

    [Fact]
    public void IsOverlapping_OverlappingTimeRangeOnDifferentMatch_ReturnsTrue()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.True(OverlapRule.IsOverlapping(participations, currentMatchId: 99, new DateOnly(2026, 9, 10), new TimeOnly(11, 0), new TimeOnly(12, 30)));
    }

    [Fact]
    public void IsOverlapping_BackToBackNoOverlap_ReturnsFalse()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.False(OverlapRule.IsOverlapping(participations, currentMatchId: 99, new DateOnly(2026, 9, 10), new TimeOnly(11, 30), new TimeOnly(13, 0)));
    }

    [Fact]
    public void IsOverlapping_DifferentDate_ReturnsFalse()
    {
        var participations = new[] { Active(5, new DateOnly(2026, 9, 10), new TimeOnly(10, 0), new TimeOnly(11, 30)) };

        Assert.False(OverlapRule.IsOverlapping(participations, currentMatchId: 99, new DateOnly(2026, 9, 11), new TimeOnly(10, 0), new TimeOnly(11, 30)));
    }
}
