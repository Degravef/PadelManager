using Bll.Rules;

namespace BllTest.Rules;

public class ReservationWindowRuleTests
{
    [Fact]
    public void IsWithinWindow_ExactlyAtWindowOpening_ReturnsTrue()
    {
        // Global member, 21-day window: booking exactly 21 days before the match is allowed.
        Assert.True(ReservationWindowRule.IsWithinWindow(21, new DateOnly(2026, 10, 1), new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void IsWithinWindow_OneDayBeforeWindowOpens_ReturnsFalse()
    {
        Assert.False(ReservationWindowRule.IsWithinWindow(21, new DateOnly(2026, 10, 1), new DateOnly(2026, 9, 9)));
    }

    [Fact]
    public void IsWithinWindow_DayBeforeMatch_ReturnsTrue()
    {
        Assert.True(ReservationWindowRule.IsWithinWindow(21, new DateOnly(2026, 10, 1), new DateOnly(2026, 9, 30)));
    }
}
