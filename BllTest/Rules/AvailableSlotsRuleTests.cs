using Bll.Rules;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class AvailableSlotsRuleTests
{
    private static SiteSchedule Schedule(TimeOnly start, TimeOnly end, int durationMinutes = 90, int breakMinutes = 15) => new()
    {
        SiteId = 1, Year = 2026, OpeningTime = start, ClosingTime = end,
        MatchDurationMinutes = durationMinutes, BreakMinutes = breakMinutes
    };

    [Fact]
    public void Calculate_WideWindow_GeneratesSlotsEvery1h45ApartStartingAtOpeningTime()
    {
        var slots = AvailableSlotsRule.Calculate(Schedule(new TimeOnly(8, 0), new TimeOnly(21, 0)));

        Assert.Equal(
        [
            new TimeOnly(8, 0), new TimeOnly(9, 45), new TimeOnly(11, 30), new TimeOnly(13, 15),
            new TimeOnly(15, 0), new TimeOnly(16, 45), new TimeOnly(18, 30), new TimeOnly(20, 15)
        ], slots);
    }

    [Fact]
    public void Calculate_LastSlotStartExactlyOnClosingHour_IsIncluded()
    {
        var slots = AvailableSlotsRule.Calculate(Schedule(new TimeOnly(8, 0), new TimeOnly(9, 45)));

        Assert.Equal([new TimeOnly(8, 0), new TimeOnly(9, 45)], slots);
    }

    [Fact]
    public void Calculate_WindowNarrowerThanOneSlot_ReturnsOnlyOpeningSlot()
    {
        var slots = AvailableSlotsRule.Calculate(Schedule(new TimeOnly(8, 0), new TimeOnly(8, 30)));

        Assert.Equal([new TimeOnly(8, 0)], slots);
    }
}
