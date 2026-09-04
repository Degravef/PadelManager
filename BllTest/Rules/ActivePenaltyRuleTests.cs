using Bll.Rules;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class ActivePenaltyRuleTests
{
    [Fact]
    public void IsActive_NoPenalties_ReturnsFalse()
    {
        Assert.False(ActivePenaltyRule.IsActive([], new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void IsActive_InactiveFlag_ReturnsFalse()
    {
        Penalty[] penalties = [new() { MemberId = 1, StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = false }];

        Assert.False(ActivePenaltyRule.IsActive(penalties, new DateOnly(2026, 9, 5)));
    }

    [Fact]
    public void IsActive_TodayBeforeEndDate_ReturnsTrue()
    {
        Penalty[] penalties = [new() { MemberId = 1, StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = true }];

        Assert.True(ActivePenaltyRule.IsActive(penalties, new DateOnly(2026, 9, 5)));
    }

    [Fact]
    public void IsActive_TodayAfterEndDate_ReturnsFalse()
    {
        Penalty[] penalties = [new() { MemberId = 1, StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = true }];

        Assert.False(ActivePenaltyRule.IsActive(penalties, new DateOnly(2026, 9, 9)));
    }
}
