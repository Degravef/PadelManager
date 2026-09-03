using Bll.Rules;

namespace BllTest.Rules;

public class DelaiReservationRuleTests
{
    [Fact]
    public void EstDansLaFenetre_ExactlyAtWindowOpening_ReturnsTrue()
    {
        
        Assert.True(DelaiReservationRule.EstDansLaFenetre(21, new DateOnly(2026, 10, 1), new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void EstDansLaFenetre_OneDayBeforeWindowOpens_ReturnsFalse()
    {
        Assert.False(DelaiReservationRule.EstDansLaFenetre(21, new DateOnly(2026, 10, 1), new DateOnly(2026, 9, 9)));
    }

    [Fact]
    public void EstDansLaFenetre_DayBeforeMatch_ReturnsTrue()
    {
        Assert.True(DelaiReservationRule.EstDansLaFenetre(21, new DateOnly(2026, 10, 1), new DateOnly(2026, 9, 30)));
    }
}
