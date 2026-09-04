using Core.Domain.Entities;

namespace Bll.Rules;

/// RG-SITE-003/004/005
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
                break;
            start = next;
        }

        return slots;
    }
}
