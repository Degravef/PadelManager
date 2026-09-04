using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>
/// RG-SITE-003/004/005: generates the site's valid match start times for one SiteSchedule — the first
/// slot starts at opening time, each following slot starts (match duration + buffer) later, and the
/// last slot's start time may not be later than the site's last-reservation hour.
/// </summary>
public static class AvailableSlotsRule
{
    public static IReadOnlyList<TimeOnly> Calculate(SiteSchedule schedule)
    {
        var slots = new List<TimeOnly>();
        TimeSpan step = TimeSpan.FromMinutes(schedule.MatchDurationMinutes + schedule.BreakMinutes);
        TimeOnly start = schedule.OpeningTime;

        while (start <= schedule.ClosingTime)
        {
            slots.Add(start);
            TimeOnly next = start.Add(step);
            if (next <= start)
                break; // wrapped past midnight — no further slots fit in the day
            start = next;
        }

        return slots;
    }
}
